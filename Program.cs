using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

// 1. Validar y obtener la clave JWT desde appsettings.json ANTES de registrar la autenticación
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("Falta configurar 'Jwt:Key' en appsettings.json");

// 2. Configurar Autenticación (Cookies + JWT)
builder.Services.AddAuthentication(options =>
{
    // Esquema por defecto para MVC (Cookies)
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/Denegado";
})
.AddJwtBearer(options =>

   options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,

    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],

    IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtKey)
    ),

    RoleClaimType = System.Security.Claims.ClaimTypes.Role
});
builder.Services.AddAuthorization();


builder.Services.AddScoped<IRepositorioSocio, RepositorioSocio>();
builder.Services.AddScoped<SocioService>();
builder.Services.AddScoped<IRepositorioProfesor, RepositorioProfesor>();
builder.Services.AddScoped<ProfesorService>();
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<IRepositorio<Plan>, RepositorioPlan>();
builder.Services.AddScoped<PlanService>();
builder.Services.AddScoped<IRepositorio<Actividad>, RepositorioActividad>();
builder.Services.AddScoped<ActividadService>();
builder.Services.AddScoped<IRepositorioMembresia, RepositorioMembresia>();
builder.Services.AddScoped<MembresiaService>();

var app = builder.Build();

// Configure  pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();