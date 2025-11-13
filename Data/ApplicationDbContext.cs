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

        public DbSet<Jogador> Jogadores { get; set; } = null!;
        public DbSet<Equipa> Equipas { get; set; } = null!;
        public DbSet<Contrato> Contratos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar precisão do decimal para Salario
            builder.Entity<Contrato>()
                .Property(c => c.Salario)
                .HasPrecision(18, 2);

            // Configurações das relações
            builder.Entity<Contrato>()
                .HasOne(c => c.Jogador)
                .WithMany(j => j.Contratos)
                .HasForeignKey(c => c.JogadorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Contrato>()
                .HasOne(c => c.Equipa)
                .WithMany(e => e.Contratos)
                .HasForeignKey(c => c.EquipaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
