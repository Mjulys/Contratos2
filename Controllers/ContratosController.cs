using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Contratos2.Data;
using Contratos2.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Contratos2.Controllers
{
    public class ContratosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContratosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Contratos
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            IQueryable<Contrato> contratosQuery = _context.Contratos
                .Include(c => c.Jogador)
                .Include(c => c.Equipa);

            // Se não estiver autenticado, mostra todos (acesso público)
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                var isAdmin = User.IsInRole("Admin");
                var isFuncionario = User.IsInRole("Funcionario");
                var isJogador = User.IsInRole("Jogador");

                // Jogador só vê seus próprios contratos
                if (isJogador && !isAdmin && !isFuncionario && user != null)
                {
                    var jogador = await _context.Jogadores
                        .FirstOrDefaultAsync(j => j.UserId == user.Id);
                    
                    if (jogador != null)
                    {
                        contratosQuery = contratosQuery.Where(c => c.JogadorId == jogador.Id);
                    }
                    else
                    {
                        contratosQuery = contratosQuery.Where(c => false); // Nenhum contrato se não for jogador
                    }
                }
            }

            var contratos = await contratosQuery.ToListAsync();
            return View(contratos);
        }

        // GET: Contratos/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .Include(c => c.Jogador)
                .Include(c => c.Equipa)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contrato == null)
            {
                return NotFound();
            }

            // Verificar se o jogador pode ver este contrato
            var user = await _userManager.GetUserAsync(User);
            var isJogador = User.IsInRole("Jogador");
            var isAdmin = User.IsInRole("Admin");
            var isFuncionario = User.IsInRole("Funcionario");

            if (isJogador && !isAdmin && !isFuncionario)
            {
                var jogador = await _context.Jogadores
                    .FirstOrDefaultAsync(j => j.UserId == user.Id);
                
                if (jogador == null || contrato.JogadorId != jogador.Id)
                {
                    return Forbid();
                }
            }

            return View(contrato);
        }

        // GET: Contratos/Create
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Create()
        {
            ViewData["JogadorId"] = new SelectList(_context.Jogadores, "Id", "Nome");
            ViewData["EquipaId"] = new SelectList(_context.Equipas, "Id", "Nome");
            return View();
        }

        // POST: Contratos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Create([Bind("Id,JogadorId,EquipaId,DataInicio,DataFim,Salario,Clausulas")] Contrato contrato)
        {
            if (ModelState.IsValid)
            {
                contrato.DataCriacao = DateTime.Now;
                _context.Add(contrato);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JogadorId"] = new SelectList(_context.Jogadores, "Id", "Nome", contrato.JogadorId);
            ViewData["EquipaId"] = new SelectList(_context.Equipas, "Id", "Nome", contrato.EquipaId);
            return View(contrato);
        }

        // GET: Contratos/Edit/5
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato == null)
            {
                return NotFound();
            }
            ViewData["JogadorId"] = new SelectList(_context.Jogadores, "Id", "Nome", contrato.JogadorId);
            ViewData["EquipaId"] = new SelectList(_context.Equipas, "Id", "Nome", contrato.EquipaId);
            return View(contrato);
        }

        // POST: Contratos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,JogadorId,EquipaId,DataInicio,DataFim,Salario,Clausulas,DataCriacao")] Contrato contrato)
        {
            if (id != contrato.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contrato);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContratoExists(contrato.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["JogadorId"] = new SelectList(_context.Jogadores, "Id", "Nome", contrato.JogadorId);
            ViewData["EquipaId"] = new SelectList(_context.Equipas, "Id", "Nome", contrato.EquipaId);
            return View(contrato);
        }

        // GET: Contratos/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contrato = await _context.Contratos
                .Include(c => c.Jogador)
                .Include(c => c.Equipa)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contrato == null)
            {
                return NotFound();
            }

            return View(contrato);
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contrato = await _context.Contratos.FindAsync(id);
            if (contrato != null)
            {
                _context.Contratos.Remove(contrato);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContratoExists(int id)
        {
            return _context.Contratos.Any(e => e.Id == id);
        }
    }
}

