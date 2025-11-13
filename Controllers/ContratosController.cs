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
    public class ContratosController : Controller
    {
        private readonly IContratoRepository _contratoRepository;
        private readonly IJogadorRepository _jogadorRepository;
        private readonly IEquipaRepository _equipaRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ContratosController> _logger;

        public ContratosController(
            IContratoRepository contratoRepository,
            IJogadorRepository jogadorRepository,
            IEquipaRepository equipaRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<ContratosController> logger)
        {
            _contratoRepository = contratoRepository;
            _jogadorRepository = jogadorRepository;
            _equipaRepository = equipaRepository;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Contratos
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? filtro = null)
        {
            try
            {
                IEnumerable<Contrato> contratos;

                // Se não estiver autenticado, mostra apenas contratos a decorrer
                if (User.Identity?.IsAuthenticated != true)
                {
                    contratos = await _contratoRepository.GetContratosAtivosAsync();
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);
                    var isAdmin = User.IsInRole("Admin");
                    var isFuncionario = User.IsInRole("Funcionario");
                    var isJogador = User.IsInRole("Jogador");

                    // Jogador só vê seus próprios contratos
                    if (isJogador && !isAdmin && !isFuncionario && user != null)
                    {
                        var jogador = await _jogadorRepository.GetByUserIdAsync(user.Id);
                        if (jogador != null)
                        {
                            contratos = await _contratoRepository.GetByJogadorIdAsync(jogador.Id);
                        }
                        else
                        {
                            contratos = new List<Contrato>();
                        }
                    }
                    else
                    {
                        // Admin e Funcionário veem todos, com filtros opcionais
                        contratos = await _contratoRepository.GetAllWithDetailsAsync();
                        
                        if (!string.IsNullOrEmpty(filtro))
                        {
                            var hoje = DateTime.Today;
                            contratos = filtro.ToLower() switch
                            {
                                "ativos" => await _contratoRepository.GetContratosAtivosAsync(),
                                "passados" => await _contratoRepository.GetContratosPassadosAsync(),
                                "futuros" => await _contratoRepository.GetContratosFuturosAsync(),
                                _ => contratos
                            };
                        }
                    }
                }

                ViewBag.Filtro = filtro;
                return View(contratos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar contratos");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar os contratos.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Contratos/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            try
            {
                var contrato = await _contratoRepository.GetByIdWithDetailsAsync(id.Value);
                if (contrato == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                // Verificar se o jogador pode ver este contrato
                if (User.Identity?.IsAuthenticated == true)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var isJogador = User.IsInRole("Jogador");
                    var isAdmin = User.IsInRole("Admin");
                    var isFuncionario = User.IsInRole("Funcionario");

                    if (isJogador && !isAdmin && !isFuncionario && user != null)
                    {
                        var jogador = await _jogadorRepository.GetByUserIdAsync(user.Id);
                        if (jogador == null || contrato.JogadorId != jogador.Id)
                        {
                            return RedirectToAction("Forbidden", "Home");
                        }
                    }
                }
                else
                {
                    // Anónimos só podem ver contratos ativos
                    var hoje = DateTime.Today;
                    if (contrato.DataInicio > hoje || contrato.DataFim < hoje)
                    {
                        return RedirectToAction("Forbidden", "Home");
                    }
                }

                return View(contrato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao visualizar contrato {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o contrato.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Contratos/Create
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var jogadores = await _jogadorRepository.GetAllAsync();
                var equipas = await _equipaRepository.GetAllAsync();

                ViewData["JogadorId"] = new SelectList(jogadores, "Id", "Nome");
                ViewData["EquipaId"] = new SelectList(equipas, "Id", "Nome");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar formulário de criação de contrato");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o formulário.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contratos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Create([Bind("Id,JogadorId,EquipaId,DataInicio,DataFim,Salario,Clausulas")] Contrato contrato)
        {
            // Validação customizada
            if (contrato.DataFim <= contrato.DataInicio)
            {
                ModelState.AddModelError("DataFim", "A data de fim deve ser posterior à data de início.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contrato.DataCriacao = DateTime.Now;
                    await _contratoRepository.AddAsync(contrato);
                    await _contratoRepository.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Contrato criado com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar contrato");
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao criar o contrato. Tente novamente.");
                }
            }

            try
            {
                var jogadores = await _jogadorRepository.GetAllAsync();
                var equipas = await _equipaRepository.GetAllAsync();
                ViewData["JogadorId"] = new SelectList(jogadores, "Id", "Nome", contrato.JogadorId);
                ViewData["EquipaId"] = new SelectList(equipas, "Id", "Nome", contrato.EquipaId);
            }
            catch
            {
                // Ignorar erro ao carregar dados para dropdown
            }

            return View(contrato);
        }

        // GET: Contratos/Edit/5
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            try
            {
                var contrato = await _contratoRepository.GetByIdAsync(id.Value);
                if (contrato == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                var jogadores = await _jogadorRepository.GetAllAsync();
                var equipas = await _equipaRepository.GetAllAsync();
                ViewData["JogadorId"] = new SelectList(jogadores, "Id", "Nome", contrato.JogadorId);
                ViewData["EquipaId"] = new SelectList(equipas, "Id", "Nome", contrato.EquipaId);
                return View(contrato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar contrato para edição {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o contrato.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contratos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,JogadorId,EquipaId,DataInicio,DataFim,Salario,Clausulas,DataCriacao")] Contrato contrato)
        {
            if (id != contrato.Id)
            {
                return RedirectToAction("NotFound", "Home");
            }

            // Validação customizada
            if (contrato.DataFim <= contrato.DataInicio)
            {
                ModelState.AddModelError("DataFim", "A data de fim deve ser posterior à data de início.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _contratoRepository.Update(contrato);
                    await _contratoRepository.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Contrato atualizado com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _contratoRepository.ExistsAsync(c => c.Id == contrato.Id))
                    {
                        return RedirectToAction("NotFound", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "O contrato foi modificado por outro utilizador. Recarregue a página e tente novamente.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar contrato {Id}", id);
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar o contrato.");
                }
            }

            try
            {
                var jogadores = await _jogadorRepository.GetAllAsync();
                var equipas = await _equipaRepository.GetAllAsync();
                ViewData["JogadorId"] = new SelectList(jogadores, "Id", "Nome", contrato.JogadorId);
                ViewData["EquipaId"] = new SelectList(equipas, "Id", "Nome", contrato.EquipaId);
            }
            catch
            {
                // Ignorar erro
            }

            return View(contrato);
        }

        // GET: Contratos/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("NotFound", "Home");
            }

            try
            {
                var contrato = await _contratoRepository.GetByIdWithDetailsAsync(id.Value);
                if (contrato == null)
                {
                    return RedirectToAction("NotFound", "Home");
                }

                return View(contrato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar contrato para eliminação {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o contrato.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var contrato = await _contratoRepository.GetByIdAsync(id);
                if (contrato != null)
                {
                    _contratoRepository.Remove(contrato);
                    await _contratoRepository.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Contrato eliminado com sucesso.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Contrato não encontrado.";
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Erro ao eliminar contrato {Id} - possível conflito de foreign key", id);
                TempData["ErrorMessage"] = "Não é possível eliminar este contrato porque está associado a outros registos.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao eliminar contrato {Id}", id);
                TempData["ErrorMessage"] = "Ocorreu um erro ao eliminar o contrato.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

