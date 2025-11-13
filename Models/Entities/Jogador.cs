using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Contratos2.Models.Entities
{
    public class Jogador
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        [StringLength(50)]
        public string? Nacionalidade { get; set; }

        [StringLength(50)]
        public string? Posicao { get; set; }

        public string? Foto { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Navegação
        public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    }
}
