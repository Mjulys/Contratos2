using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Contratos2.Data;
using Contratos2.Models.Entities;

namespace Contratos2.Controllers
{
    [Authorize]
    public class EquipasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EquipasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Equipas
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Equipas.ToListAsync());
        }

        // GET: Equipas/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipa = await _context.Equipas
                .Include(e => e.Contratos)
                    .ThenInclude(c => c.Jogador)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (equipa == null)
            {
                return NotFound();
            }

            return View(equipa);
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
                _context.Add(equipa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(equipa);
        }

        // GET: Equipas/Edit/5
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipa = await _context.Equipas.FindAsync(id);
            if (equipa == null)
            {
                return NotFound();
            }
            return View(equipa);
        }

        // POST: Equipas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Localidade,Estadio,Emblema,DataFundacao")] Equipa equipa)
        {
            if (id != equipa.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(equipa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipaExists(equipa.Id))
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
            return View(equipa);
        }

        // GET: Equipas/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var equipa = await _context.Equipas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (equipa == null)
            {
                return NotFound();
            }

            return View(equipa);
        }

        // POST: Equipas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipa = await _context.Equipas.FindAsync(id);
            if (equipa != null)
            {
                _context.Equipas.Remove(equipa);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EquipaExists(int id)
        {
            return _context.Equipas.Any(e => e.Id == id);
        }
    }
}

