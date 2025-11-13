using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Contratos2.Models.Entities;

namespace Contratos2.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Verificar se já existem dados
            if (await context.Equipas.AnyAsync())
            {
                return; // Já foi inicializado
            }

            // Criar Equipas
            var equipas = new List<Equipa>
            {
                new Equipa { Nome = "FC Porto" },
                new Equipa { Nome = "Sporting CP" },
                new Equipa { Nome = "SL Benfica" },
                new Equipa { Nome = "SC Braga" },
                new Equipa { Nome = "Vitória SC" },
                new Equipa { Nome = "FC Paços de Ferreira" },
                new Equipa { Nome = "Rio Ave FC" },
                new Equipa { Nome = "FC Famalicão" }
            };

            context.Equipas.AddRange(equipas);
            await context.SaveChangesAsync();

            // Criar Jogadores e Utilizadores
            var jogadores = new List<(ApplicationUser User, Jogador Jogador)>();
            var random = new Random();

            var nomesJogadores = new[]
            {
                ("Cristiano Ronaldo", "cristiano@example.com", new DateTime(1985, 2, 5)),
                ("Lionel Messi", "messi@example.com", new DateTime(1987, 6, 24)),
                ("Neymar Jr", "neymar@example.com", new DateTime(1992, 2, 5)),
                ("Kylian Mbappé", "mbappe@example.com", new DateTime(1998, 12, 20)),
                ("Erling Haaland", "haaland@example.com", new DateTime(2000, 7, 21)),
                ("Kevin De Bruyne", "debruyne@example.com", new DateTime(1991, 6, 28)),
                ("Mohamed Salah", "salah@example.com", new DateTime(1992, 6, 15)),
                ("Virgil van Dijk", "vandijk@example.com", new DateTime(1991, 7, 8)),
                ("Bruno Fernandes", "bruno@example.com", new DateTime(1994, 9, 8)),
                ("Bernardo Silva", "bernardo@example.com", new DateTime(1994, 8, 10)),
                ("Rúben Dias", "ruben@example.com", new DateTime(1997, 5, 14)),
                ("João Félix", "joaofelix@example.com", new DateTime(1999, 11, 10)),
                ("Rafael Leão", "leao@example.com", new DateTime(1999, 6, 10)),
                ("Diogo Jota", "jota@example.com", new DateTime(1996, 12, 4)),
                ("Gonçalo Ramos", "ramos@example.com", new DateTime(2001, 6, 20)),
                ("Vitinha", "vitinha@example.com", new DateTime(2000, 2, 13)),
                ("Pedro Gonçalves", "pedro@example.com", new DateTime(1998, 6, 28)),
                ("Nuno Mendes", "nunomendes@example.com", new DateTime(2002, 6, 19)),
                ("António Silva", "antonio@example.com", new DateTime(2003, 10, 30)),
                ("Diogo Costa", "diogocosta@example.com", new DateTime(1999, 9, 19))
            };

            foreach (var (nome, email, dataNascimento) in nomesJogadores)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    NomeCompleto = nome,
                    TipoUtilizador = "Jogador",
                    DataRegisto = DateTime.Now.AddDays(-random.Next(30, 365))
                };

                var password = "Jogador123!";
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Jogador");

                    var jogador = new Jogador
                    {
                        Nome = nome,
                        Email = email,
                        DataNascimento = dataNascimento,
                        Nacionalidade = "Portuguesa",
                        Posicao = new[] { "Avançado", "Médio", "Defesa", "Guarda-Redes" }[random.Next(4)],
                        UserId = user.Id
                    };

                    context.Jogadores.Add(jogador);
                    jogadores.Add((user, jogador));
                }
            }

            await context.SaveChangesAsync();

            // Criar Contratos
            var hoje = DateTime.Today;
            var contratos = new List<Contrato>();

            foreach (var (user, jogador) in jogadores)
            {
                // Contrato atual
                var equipaAtual = equipas[random.Next(equipas.Count)];
                var dataInicioAtual = hoje.AddMonths(-random.Next(1, 12));
                var dataFimAtual = dataInicioAtual.AddYears(random.Next(1, 3));

                contratos.Add(new Contrato
                {
                    JogadorId = jogador.Id,
                    EquipaId = equipaAtual.Id,
                    DataInicio = dataInicioAtual,
                    DataFim = dataFimAtual,
                    Salario = random.Next(5000, 50000),
                    Clausulas = $"Cláusula de rescisão: {random.Next(100000, 5000000)}€",
                    DataCriacao = dataInicioAtual.AddDays(-random.Next(1, 30))
                });

                // Contrato passado (50% de chance)
                if (random.Next(2) == 0)
                {
                    var equipaPassada = equipas[random.Next(equipas.Count)];
                    var dataFimPassada = hoje.AddMonths(-random.Next(1, 24));
                    var dataInicioPassada = dataFimPassada.AddYears(-random.Next(1, 3));

                    contratos.Add(new Contrato
                    {
                        JogadorId = jogador.Id,
                        EquipaId = equipaPassada.Id,
                        DataInicio = dataInicioPassada,
                        DataFim = dataFimPassada,
                        Salario = random.Next(3000, 40000),
                        Clausulas = $"Cláusula de rescisão: {random.Next(50000, 3000000)}€",
                        DataCriacao = dataInicioPassada.AddDays(-random.Next(1, 30))
                    });
                }

                // Contrato futuro (30% de chance)
                if (random.Next(10) < 3)
                {
                    var equipaFutura = equipas[random.Next(equipas.Count)];
                    var dataInicioFutura = hoje.AddMonths(random.Next(1, 12));
                    var dataFimFutura = dataInicioFutura.AddYears(random.Next(1, 3));

                    contratos.Add(new Contrato
                    {
                        JogadorId = jogador.Id,
                        EquipaId = equipaFutura.Id,
                        DataInicio = dataInicioFutura,
                        DataFim = dataFimFutura,
                        Salario = random.Next(6000, 60000),
                        Clausulas = $"Cláusula de rescisão: {random.Next(150000, 6000000)}€",
                        DataCriacao = hoje.AddDays(-random.Next(1, 90))
                    });
                }
            }

            context.Contratos.AddRange(contratos);
            await context.SaveChangesAsync();
        }
    }
}

