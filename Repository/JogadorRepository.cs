using Microsoft.EntityFrameworkCore;
using Contratos2.Data;
using Contratos2.Models.Entities;

namespace Contratos2.Repository
{
    public class JogadorRepository : Repository<Jogador>, IJogadorRepository
    {
        public JogadorRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Jogador>> GetAllWithUserAsync()
        {
            return await _dbSet
                .Include(j => j.User)
                .ToListAsync();
        }

        public async Task<Jogador?> GetByIdWithContratosAsync(int id)
        {
            return await _dbSet
                .Include(j => j.User)
                .Include(j => j.Contratos)
                    .ThenInclude(c => c.Equipa)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<Jogador?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(j => j.UserId == userId);
        }
    }
}

