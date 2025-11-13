using Microsoft.EntityFrameworkCore;
using Contratos2.Data;
using Microsoft.AspNetCore.Identity;
using Contratos2.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    
    // Email confirmation
    options.User.RequireUniqueEmail = true;
    
    // Password recovery
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Email sender (para desenvolvimento)
builder.Services.AddTransient<Contratos2.Services.IEmailSender, Contratos2.Services.EmailSender>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Inicializar roles e usuário admin
using (var scope = app.Services.CreateScope())
{
    await Contratos2.Services.RoleInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();