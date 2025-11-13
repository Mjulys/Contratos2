using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Contratos2.Data;
using Contratos2.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Contratos2.Controllers
{
    [Authorize]
    public class JogadoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JogadoresController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Jogadores
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var jogadores = await _context.Jogadores
                .Include(j => j.User)
                .ToListAsync();
            return View(jogadores);
        }

        // GET: Jogadores/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jogador = await _context.Jogadores
                .Include(j => j.User)
                .Include(j => j.Contratos)
                    .ThenInclude(c => c.Equipa)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (jogador == null)
            {
                return NotFound();
            }

            return View(jogador);
        }

        // GET: Jogadores/Create
        [Authorize(Roles = "Admin,Funcionario")]
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Jogadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Create([Bind("Id,Nome,Email,DataNascimento,Nacionalidade,Posicao,Foto,UserId")] Jogador jogador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jogador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
                return NotFound();
            }

            var jogador = await _context.Jogadores.FindAsync(id);
            if (jogador == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", jogador.UserId);
            return View(jogador);
        }

        // POST: Jogadores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,DataNascimento,Nacionalidade,Posicao,Foto,UserId")] Jogador jogador)
        {
            if (id != jogador.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jogador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JogadorExists(jogador.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", jogador.UserId);
            return View(jogador);
        }

        // GET: Jogadores/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jogador = await _context.Jogadores
                .Include(j => j.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jogador == null)
            {
                return NotFound();
            }

            return View(jogador);
        }

        // POST: Jogadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var jogador = await _context.Jogadores.FindAsync(id);
            if (jogador != null)
            {
                _context.Jogadores.Remove(jogador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JogadorExists(int id)
        {
            return _context.Jogadores.Any(e => e.Id == id);
        }
    }
}

