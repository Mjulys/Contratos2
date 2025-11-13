using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contratos2.Models.Entities;
using Contratos2.Repository;
using Microsoft.Extensions.Logging;

namespace Contratos2.Controllers
{
    [Authorize]
    public class EquipasController : Controller
    {
        private readonly IEquipaRepository _equipaRepository;
        private readonly ILogger<EquipasController> _logger;

        public EquipasController(IEquipaRepository equipaRepository, ILogger<EquipasController> logger)
        {
            _equipaRepository = equipaRepository;
            _logger = logger;
        }

        // GET: Equipas
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var equipas = await _equipaRepository.GetAllAsync();
                return View(equipas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar equipas");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar as equipas.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Equipas/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            try
            {
                var equipa = await _equipaRepository.GetByIdWithContratosAsync(id.Value);
                if (equipa == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                return View(equipa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao visualizar equipa {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a equipa.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Equipas/Create
        [Authorize(Roles = "Admin,Funcionario")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Equipas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Create([Bind("Id,Nome,Localidade,Estadio,Emblema,DataFundacao")] Equipa equipa)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _equipaRepository.AddAsync(equipa);
                    await _equipaRepository.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Equipa criada com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar equipa");
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao criar a equipa. Tente novamente.");
                }
            }

            return View(equipa);
        }

        // GET: Equipas/Edit/5
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            try
            {
                var equipa = await _equipaRepository.GetByIdAsync(id.Value);
                if (equipa == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                return View(equipa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar equipa para edição {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a equipa.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Equipas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Localidade,Estadio,Emblema,DataFundacao")] Equipa equipa)
        {
            if (id != equipa.Id)
            {
                return RedirectToAction("NotFound", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _equipaRepository.Update(equipa);
                    await _equipaRepository.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Equipa atualizada com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _equipaRepository.ExistsAsync(e => e.Id == equipa.Id))
                    {
                        return RedirectToAction("NotFound", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "A equipa foi modificada por outro utilizador. Recarregue a página e tente novamente.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar equipa {Id}", id);
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar a equipa.");
                }
            }

            return View(equipa);
        }

        // GET: Equipas/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            try
            {
                var equipa = await _equipaRepository.GetByIdWithContratosAsync(id.Value);
                if (equipa == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                // Verificar se tem contratos
                if (equipa.Contratos != null && equipa.Contratos.Any())
                {
                    TempData["ErrorMessage"] = "Não é possível eliminar esta equipa porque possui contratos associados.";
                    return RedirectToAction(nameof(Index));
                }

                return View(equipa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar equipa para eliminação {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar a equipa.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Equipas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var equipa = await _equipaRepository.GetByIdAsync(id);
                if (equipa != null)
                {
                    // Verificar se tem contratos
                    var equipaComContratos = await _equipaRepository.GetByIdWithContratosAsync(id);
                    if (equipaComContratos != null && equipaComContratos.Contratos != null && equipaComContratos.Contratos.Any())
                    {
                        TempData["ErrorMessage"] = "Não é possível eliminar esta equipa porque possui contratos associados.";
                        return RedirectToAction(nameof(Index));
                    }

                    _equipaRepository.Remove(equipa);
                    await _equipaRepository.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Equipa eliminada com sucesso.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Equipa não encontrada.";
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao eliminar equipa {Id} - possível conflito de foreign key", id);
                TempData["ErrorMessage"] = "Não é possível eliminar esta equipa porque está associada a outros registos.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao eliminar equipa {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao eliminar a equipa.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

