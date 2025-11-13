using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Contratos2.Models;

namespace Contratos2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            if (exception != null)
            {
                _logger.LogError(exception, "Erro não tratado ocorreu");
            }

            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = exception?.Message ?? (statusCode.HasValue ? GetErrorMessage(statusCode.Value) : "Ocorreu um erro inesperado.")
            };

            if (statusCode.HasValue)
            {
                Response.StatusCode = statusCode.Value;
                
                return statusCode.Value switch
                {
                    404 => RedirectToAction("NotFound"),
                    403 => RedirectToAction("Forbidden"),
                    500 => RedirectToAction("ServerError"),
                    _ => View(errorViewModel)
                };
            }

            return View(errorViewModel);
        }

        private string GetErrorMessage(int statusCode)
        {
            return statusCode switch
            {
                404 => "A página solicitada não foi encontrada.",
                403 => "Não tem permissão para aceder a esta página.",
                500 => "Ocorreu um erro interno no servidor.",
                _ => "Ocorreu um erro inesperado."
            };
        }

        public IActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        public IActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View();
        }

        public IActionResult ServerError()
        {
            Response.StatusCode = 500;
            return View();
        }
    }
}
