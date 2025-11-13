using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Contratos2.Models.Entities;
using Contratos2.Services;

namespace Contratos2.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<PerfilController> _logger;

        public PerfilController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ILogger<PerfilController> logger)
        {
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }

        // GET: Perfil
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Perfil/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Perfil/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,NomeCompleto,FotoPerfil")] ApplicationUser model, IFormFile? fotoFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.NomeCompleto = model.NomeCompleto;

                    // Upload de foto
                    if (fotoFile != null && fotoFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "perfis");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(fotoFile.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("FotoPerfil", "Apenas ficheiros de imagem são permitidos (JPG, PNG, GIF).");
                            return View(user);
                        }

                        var fileName = $"{user.Id}_{Guid.NewGuid()}{fileExtension}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await fotoFile.CopyToAsync(stream);
                        }

                        // Remover foto antiga se existir
                        if (!string.IsNullOrEmpty(user.FotoPerfil) && user.FotoPerfil.StartsWith("/uploads/"))
                        {
                            var oldFilePath = Path.Combine(_environment.WebRootPath, user.FotoPerfil.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        user.FotoPerfil = $"/uploads/perfis/{fileName}";
                    }

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Perfil atualizado com sucesso.";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar perfil");
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar o perfil.");
                }
            }

            return View(user);
        }
    }
}

