using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Contratos2.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string NomeCompleto { get; set; } = string.Empty;

        public string? FotoPerfil { get; set; }

        [Required]
        public string TipoUtilizador { get; set; } = "Jogador"; // Admin, Funcionario, Jogador

        public DateTime DataRegisto { get; set; } = DateTime.Now;

        // Navegação
        public Jogador? Jogador { get; set; }
    }
}
