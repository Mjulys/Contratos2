using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Contratos2.Models.Entities;
using Contratos2.Repository;
using Contratos2.Data;
using Microsoft.EntityFrameworkCore;

namespace Contratos2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IContratoRepository _contratoRepository;
        private readonly IJogadorRepository _jogadorRepository;
        private readonly IEquipaRepository _equipaRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IContratoRepository contratoRepository,
            IJogadorRepository jogadorRepository,
            IEquipaRepository equipaRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DashboardController> logger)
        {
            _contratoRepository = contratoRepository;
            _jogadorRepository = jogadorRepository;
            _equipaRepository = equipaRepository;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var hoje = DateTime.Today;

                // Estatísticas gerais
                var totalJogadores = await _jogadorRepository.GetAllAsync();
                var totalEquipas = await _equipaRepository.GetAllAsync();
                var todosContratos = await _contratoRepository.GetAllWithDetailsAsync();
                var contratosAtivos = await _contratoRepository.GetContratosAtivosAsync();
                var contratosPassados = await _contratoRepository.GetContratosPassadosAsync();
                var contratosFuturos = await _contratoRepository.GetContratosFuturosAsync();

                ViewBag.TotalJogadores = totalJogadores.Count();
                ViewBag.TotalEquipas = totalEquipas.Count();
                ViewBag.TotalContratos = todosContratos.Count();
                ViewBag.ContratosAtivos = contratosAtivos.Count();
                ViewBag.ContratosPassados = contratosPassados.Count();
                ViewBag.ContratosFuturos = contratosFuturos.Count();

                // Salário total dos contratos ativos
                var salarioTotalAtivos = contratosAtivos
                    .Where(c => c.Salario.HasValue)
                    .Sum(c => c.Salario!.Value);
                ViewBag.SalarioTotalAtivos = salarioTotalAtivos;

                // Salário médio
                var salarioMedio = todosContratos
                    .Where(c => c.Salario.HasValue)
                    .Select(c => c.Salario!.Value)
                    .DefaultIfEmpty(0)
                    .Average();
                ViewBag.SalarioMedio = salarioMedio;

                // Contratos por mês (últimos 12 meses)
                var contratosPorMes = todosContratos
                    .Where(c => c.DataCriacao >= hoje.AddMonths(-12))
                    .GroupBy(c => new { c.DataCriacao.Year, c.DataCriacao.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new
                    {
                        Mes = $"{g.Key.Month:00}/{g.Key.Year}",
                        Quantidade = g.Count()
                    })
                    .ToList();

                ViewBag.ContratosPorMesLabels = contratosPorMes.Select(c => c.Mes).ToList();
                ViewBag.ContratosPorMesData = contratosPorMes.Select(c => c.Quantidade).ToList();

                // Jogadores por equipa (top 10)
                var jogadoresPorEquipa = await _context.Contratos
                    .Include(c => c.Equipa)
                    .Where(c => c.DataInicio <= hoje && c.DataFim >= hoje)
                    .GroupBy(c => new { c.EquipaId, c.Equipa!.Nome })
                    .Select(g => new
                    {
                        Equipa = g.Key.Nome,
                        Quantidade = g.Select(c => c.JogadorId).Distinct().Count()
                    })
                    .OrderByDescending(g => g.Quantidade)
                    .Take(10)
                    .ToListAsync();

                ViewBag.JogadoresPorEquipaLabels = jogadoresPorEquipa.Select(e => e.Equipa).ToList();
                ViewBag.JogadoresPorEquipaData = jogadoresPorEquipa.Select(e => e.Quantidade).ToList();

                // Distribuição de salários (faixas)
                var faixasSalario = new[]
                {
                    new { Min = 0m, Max = 5000m, Label = "0 - 5.000€" },
                    new { Min = 5000m, Max = 10000m, Label = "5.000€ - 10.000€" },
                    new { Min = 10000m, Max = 20000m, Label = "10.000€ - 20.000€" },
                    new { Min = 20000m, Max = 50000m, Label = "20.000€ - 50.000€" },
                    new { Min = 50000m, Max = decimal.MaxValue, Label = "> 50.000€" }
                };

                var distribuicaoSalarios = faixasSalario.Select(faixa =>
                {
                    var count = contratosAtivos
                        .Where(c => c.Salario.HasValue && c.Salario >= faixa.Min && c.Salario < faixa.Max)
                        .Count();
                    return new { Faixa = faixa.Label, Quantidade = count };
                }).ToList();

                ViewBag.DistribuicaoSalariosLabels = distribuicaoSalarios.Select(d => d.Faixa).ToList();
                ViewBag.DistribuicaoSalariosData = distribuicaoSalarios.Select(d => d.Quantidade).ToList();

                // Contratos que expiram nos próximos 3 meses
                var contratosExpirando = todosContratos
                    .Where(c => c.DataFim >= hoje && c.DataFim <= hoje.AddMonths(3))
                    .OrderBy(c => c.DataFim)
                    .Take(10)
                    .ToList();

                ViewBag.ContratosExpirando = contratosExpirando;

                // Top 5 jogadores com mais contratos
                var topJogadores = await _context.Contratos
                    .Include(c => c.Jogador)
                    .GroupBy(c => new { c.JogadorId, c.Jogador!.Nome })
                    .Select(g => new
                    {
                        Jogador = g.Key.Nome,
                        Quantidade = g.Count()
                    })
                    .OrderByDescending(g => g.Quantidade)
                    .Take(5)
                    .ToListAsync();

                ViewBag.TopJogadores = topJogadores;

                // Total de utilizadores
                var totalUtilizadores = await _userManager.Users.CountAsync();
                ViewBag.TotalUtilizadores = totalUtilizadores;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}

