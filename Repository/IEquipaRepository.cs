using Contratos2.Models.Entities;

namespace Contratos2.Repository
{
    public interface IEquipaRepository : IRepository<Equipa>
    {
        Task<IEnumerable<Equipa>> GetAllWithContratosAsync();
        Task<Equipa?> GetByIdWithContratosAsync(int id);
    }
}

