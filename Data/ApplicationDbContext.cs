using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Contratos2.Models.Entities;

namespace Contratos2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Jogador> Jogadores { get; set; }
        public DbSet<Equipa> Equipas { get; set; }
        public DbSet<Contrato> Contratos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar precisão do decimal para Salario
            builder.Entity<Contrato>()
                .Property(c => c.Salario)
                .HasPrecision(18, 2);
        }
    }
}
