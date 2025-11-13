using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Contratos2.Models.Entities;
using Contratos2.Services;

namespace Contratos2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UtilizadoresController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UtilizadoresController> _logger;

        public UtilizadoresController(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<UtilizadoresController> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // GET: Utilizadores
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // GET: Utilizadores/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;
            return View(user);
        }

        // GET: Utilizadores/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(new[] { "Funcionario", "Jogador" }, "Funcionario");
            return View();
        }

        // POST: Utilizadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NomeCompleto,Email,TipoUtilizador")] ApplicationUser user, string role)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    user.UserName = user.Email;
                    user.EmailConfirmed = false;
                    user.DataRegisto = DateTime.Now;

                    // Criar utilizador sem password - será obrigado a criar no primeiro login
                    var tempPassword = Guid.NewGuid().ToString() + "A1@";
                    var result = await _userManager.CreateAsync(user, tempPassword);

                    if (result.Succeeded)
                    {
                        // Adicionar role
                        if (!string.IsNullOrEmpty(role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }

                        // Gerar token para alterar password (primeiro login)
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        token = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));
                        var callbackUrl = Url.Page(
                            "/Account/ChangePassword",
                            pageHandler: null,
                            values: new { area = "Identity", code = token, email = user.Email },
                            protocol: Request.Scheme);

                        // Enviar email para alterar password
                        await _emailSender.SendEmailAsync(
                            user.Email!,
                            "Bem-vindo - Defina a sua password",
                            $"Olá {user.NomeCompleto},<br><br>" +
                            $"Uma conta foi criada para si no sistema de Gestão de Contratos Desportivos.<br><br>" +
                            $"Por favor, defina a sua password <a href='{callbackUrl}'>clicando aqui</a>.<br><br>" +
                            $"Este link é válido por 24 horas.<br><br>" +
                            $"Nota: Sem definir a password, não poderá fazer login na plataforma.<br><br>" +
                            $"Cumprimentos,<br>Equipa de Gestão");

                        TempData["SuccessMessage"] = $"Utilizador criado com sucesso. Email enviado para {user.Email}";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar utilizador");
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao criar o utilizador. Tente novamente.");
                }
            }

            ViewBag.Roles = new SelectList(new[] { "Funcionario", "Jogador" }, role);
            return View(user);
        }

        // GET: Utilizadores/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.CurrentRole = roles.FirstOrDefault();
            ViewBag.Roles = new SelectList(new[] { "Funcionario", "Jogador" }, roles.FirstOrDefault());
            return View(user);
        }

        // POST: Utilizadores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,NomeCompleto,Email,TipoUtilizador,FotoPerfil")] ApplicationUser user, string role)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _userManager.FindByIdAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.NomeCompleto = user.NomeCompleto;
                    existingUser.TipoUtilizador = user.TipoUtilizador;
                    existingUser.FotoPerfil = user.FotoPerfil;

                    var result = await _userManager.UpdateAsync(existingUser);

                    if (result.Succeeded)
                    {
                        // Atualizar role
                        var currentRoles = await _userManager.GetRolesAsync(existingUser);
                        if (currentRoles.Any())
                        {
                            await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                        }
                        if (!string.IsNullOrEmpty(role))
                        {
                            await _userManager.AddToRoleAsync(existingUser, role);
                        }

                        TempData["SuccessMessage"] = "Utilizador atualizado com sucesso.";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar utilizador");
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar o utilizador.");
                }
            }

            ViewBag.Roles = new SelectList(new[] { "Funcionario", "Jogador" }, role);
            return View(user);
        }

        // GET: Utilizadores/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;
            return View(user);
        }

        // POST: Utilizadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Utilizador eliminado com sucesso.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Erro ao eliminar utilizador.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao eliminar utilizador");
                TempData["ErrorMessage"] = "Ocorreu um erro ao eliminar o utilizador.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

