
using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly IConfiguration _configuration;

        public CuentaController(
            UsuarioService usuarioService,
            IConfiguration configuration)
        {
            _usuarioService = usuarioService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Mensaje =
                    "Debe ingresar email y password.";

                return View();
            }

            var usuario =
                _usuarioService.ObtenerPorEmail(email);

            if (usuario == null ||
                !usuario.Estado ||
                usuario.Password != password)
            {
                ViewBag.Mensaje =
                    "Email o password incorrectos.";

                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.IdUsuario.ToString()
                ),
                new Claim(
                    ClaimTypes.Name,
                    usuario.Email ?? ""
                ),
                new Claim(
                    ClaimTypes.Role,
                    usuario.Roles.ToString()
                )
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identidad);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }

        // Emite un JWT para el usuario que ya inició sesión.
        [HttpGet]
        [Authorize]
        public IActionResult Token()
        {
            var id = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var email = User.FindFirstValue(
                ClaimTypes.Name);

            var rol = User.FindFirstValue(
                ClaimTypes.Role);

            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(rol))
            {
                return Unauthorized(new
                {
                    mensaje = "La sesión no contiene los datos necesarios."
                });
            }

            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key) ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
            {
                return StatusCode(500, new
                {
                    mensaje = "La configuración JWT está incompleta."
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, rol)
            };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credenciales = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credenciales
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler()
                    .WriteToken(jwt)
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Login", "Cuenta");
        }

        [HttpGet]
        public IActionResult Denegado()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult EditarPerfil()
        {
            return View();
        }




    }





}
