using System.ComponentModel.DataAnnotations;

namespace Contratos2.Models.Entities
{
    public class Equipa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(100)]
        public string? Localidade { get; set; }

        [StringLength(100)]
        public string? Estadio { get; set; }

        public string? Emblema { get; set; }

        public DateTime DataFundacao { get; set; }

        // Navegação
        public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    }
}
