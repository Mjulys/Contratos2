using Microsoft.EntityFrameworkCore;
using Contratos2.Data;
using Contratos2.Models.Entities;

namespace Contratos2.Repository
{
    public class EquipaRepository : Repository<Equipa>, IEquipaRepository
    {
        public EquipaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Equipa>> GetAllWithContratosAsync()
        {
            return await _dbSet
                .Include(e => e.Contratos)
                    .ThenInclude(c => c.Jogador)
                .ToListAsync();
        }

        public async Task<Equipa?> GetByIdWithContratosAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Contratos)
                    .ThenInclude(c => c.Jogador)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}

