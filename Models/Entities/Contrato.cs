using System.ComponentModel.DataAnnotations;

namespace GestaoContratosDesportivos.Models.Entities
{
    public class Contrato
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JogadorId { get; set; }
        public Jogador Jogador { get; set; }

        [Required]
        public int EquipaId { get; set; }
        public Equipa Equipa { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataFim { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Salario { get; set; }

        [StringLength(500)]
        public string? Clausulas { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
