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

// Repository Pattern
builder.Services.AddScoped(typeof(Contratos2.Repository.IRepository<>), typeof(Contratos2.Repository.Repository<>));
builder.Services.AddScoped<Contratos2.Repository.IJogadorRepository, Contratos2.Repository.JogadorRepository>();
builder.Services.AddScoped<Contratos2.Repository.IEquipaRepository, Contratos2.Repository.EquipaRepository>();
builder.Services.AddScoped<Contratos2.Repository.IContratoRepository, Contratos2.Repository.ContratoRepository>();

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

// Tratamento de erros HTTP
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Inicializar roles, usuário admin e dados de exemplo
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await Contratos2.Services.RoleInitializer.InitializeAsync(services);
    
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DbInitializer.InitializeAsync(context, userManager, roleManager);
}

app.Run();
