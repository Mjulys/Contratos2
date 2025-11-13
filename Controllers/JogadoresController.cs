using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Contratos2.Data;
using Contratos2.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Contratos2.Repository;

namespace Contratos2.Controllers
{
    [Authorize]
    public class JogadoresController : Controller
    {
        private readonly IJogadorRepository _jogadorRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<JogadoresController> _logger;

        public JogadoresController(
            IJogadorRepository jogadorRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<JogadoresController> logger)
        {
            _jogadorRepository = jogadorRepository;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Jogadores
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var jogadores = await _jogadorRepository.GetAllWithUserAsync();
                return View(jogadores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar jogadores");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar os jogadores.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Jogadores/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            try
            {
                var jogador = await _jogadorRepository.GetByIdWithContratosAsync(id.Value);
                if (jogador == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                return View(jogador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao visualizar jogador {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o jogador.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Jogadores/Create
        [Authorize(Roles = "Admin,Funcionario")]
        public IActionResult Create()
        {
            try
            {
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar formulário de criação de jogador");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o formulário.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Jogadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Create([Bind("Id,Nome,Email,DataNascimento,Nacionalidade,Posicao,Foto,UserId")] Jogador jogador)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _jogadorRepository.AddAsync(jogador);
                    await _jogadorRepository.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Jogador criado com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar jogador");
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao criar o jogador. Tente novamente.");
                }
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", jogador.UserId);
            return View(jogador);
        }

        // GET: Jogadores/Edit/5
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            try
            {
                var jogador = await _jogadorRepository.GetByIdAsync(id.Value);
                if (jogador == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", jogador.UserId);
                return View(jogador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar jogador para edição {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o jogador.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Jogadores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,DataNascimento,Nacionalidade,Posicao,Foto,UserId")] Jogador jogador)
        {
            if (id != jogador.Id)
            {
                return RedirectToAction("NotFound", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _jogadorRepository.Update(jogador);
                    await _jogadorRepository.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Jogador atualizado com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _jogadorRepository.ExistsAsync(j => j.Id == jogador.Id))
                    {
                        return RedirectToAction("NotFound", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "O jogador foi modificado por outro utilizador. Recarregue a página e tente novamente.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar jogador {Id}", id);
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar o jogador.");
                }
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", jogador.UserId);
            return View(jogador);
        }

        // GET: Jogadores/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            try
            {
                var jogador = await _jogadorRepository.GetByIdWithContratosAsync(id.Value);
                if (jogador == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                // Verificar se tem contratos
                if (jogador.Contratos != null && jogador.Contratos.Any())
                {
                    TempData["ErrorMessage"] = "Não é possível eliminar este jogador porque possui contratos associados.";
                    return RedirectToAction(nameof(Index));
                }

                return View(jogador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar jogador para eliminação {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o jogador.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Jogadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var jogador = await _jogadorRepository.GetByIdAsync(id);
                if (jogador != null)
                {
                    // Verificar se tem contratos
                    var jogadorComContratos = await _jogadorRepository.GetByIdWithContratosAsync(id);
                    if (jogadorComContratos != null && jogadorComContratos.Contratos != null && jogadorComContratos.Contratos.Any())
                    {
                        TempData["ErrorMessage"] = "Não é possível eliminar este jogador porque possui contratos associados.";
                        return RedirectToAction(nameof(Index));
                    }

                    _jogadorRepository.Remove(jogador);
                    await _jogadorRepository.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Jogador eliminado com sucesso.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Jogador não encontrado.";
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao eliminar jogador {Id} - possível conflito de foreign key", id);
                TempData["ErrorMessage"] = "Não é possível eliminar este jogador porque está associado a outros registos.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao eliminar jogador {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao eliminar o jogador.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
