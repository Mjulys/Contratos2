using Microsoft.EntityFrameworkCore;
using Contratos2.Data;
using Contratos2.Models.Entities;

namespace Contratos2.Repository
{
    public class ContratoRepository : Repository<Contrato>, IContratoRepository
    {
        public ContratoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Contrato>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(c => c.Jogador)
                .Include(c => c.Equipa)
                .ToListAsync();
        }

        public async Task<Contrato?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Jogador)
                .Include(c => c.Equipa)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Contrato>> GetByJogadorIdAsync(int jogadorId)
        {
            return await _dbSet
                .Include(c => c.Jogador)
                .Include(c => c.Equipa)
                .Where(c => c.JogadorId == jogadorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contrato>> GetByEquipaIdAsync(int equipaId)
        {
            return await _dbSet
                .Include(c => c.Jogador)
                .Include(c => c.Equipa)
                .Where(c => c.EquipaId == equipaId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contrato>> GetContratosAtivosAsync()
        {
            var hoje = DateTime.Today;
            return await _dbSet
                .Include(c => c.Jogador)
                .Include(c => c.Equipa)
                .Where(c => c.DataInicio <= hoje && c.DataFim >= hoje)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contrato>> GetContratosPassadosAsync()
        {
            var hoje = DateTime.Today;
            return await _dbSet
                .Include(c => c.Jogador)
                .Include(c => c.Equipa)
                .Where(c => c.DataFim < hoje)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contrato>> GetContratosFuturosAsync()
        {
            var hoje = DateTime.Today;
            return await _dbSet
                .Include(c => c.Jogador)
                .Include(c => c.Equipa)
                .Where(c => c.DataInicio > hoje)
                .ToListAsync();
        }
    }
}

