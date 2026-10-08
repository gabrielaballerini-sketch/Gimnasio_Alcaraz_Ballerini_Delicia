using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Gimnasio_Alcaraz_Ballerini_Delicia.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Gimnasio_Alcaraz_Ballerini_Delicia.DAO;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

// autorizacion x cookiess

builder.Services.AddAuthentication(options =>
{
    // Opcional: Define un esquema por defecto si lo requieres
})
.AddCookie(options =>
{
    options.LoginPath = "/Cuenta/Login";
    options.AccessDeniedPath = "/Cuenta/Denegado";
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Asegúrate de tener esto en tu appsettings.json o pon el valor directo
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
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


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
