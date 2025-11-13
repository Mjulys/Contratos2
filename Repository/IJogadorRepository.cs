using Contratos2.Models.Entities;

namespace Contratos2.Repository
{
    public interface IJogadorRepository : IRepository<Jogador>
    {
        Task<IEnumerable<Jogador>> GetAllWithUserAsync();
        Task<Jogador?> GetByIdWithContratosAsync(int id);
        Task<Jogador?> GetByUserIdAsync(string userId);
    }
}

