using Contratos2.Models.Entities;

namespace Contratos2.Repository
{
    public interface IContratoRepository : IRepository<Contrato>
    {
        Task<IEnumerable<Contrato>> GetAllWithDetailsAsync();
        Task<Contrato?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Contrato>> GetByJogadorIdAsync(int jogadorId);
        Task<IEnumerable<Contrato>> GetByEquipaIdAsync(int equipaId);
        Task<IEnumerable<Contrato>> GetContratosAtivosAsync();
        Task<IEnumerable<Contrato>> GetContratosPassadosAsync();
        Task<IEnumerable<Contrato>> GetContratosFuturosAsync();
    }
}

