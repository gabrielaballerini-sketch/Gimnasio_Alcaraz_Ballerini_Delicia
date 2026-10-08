using Gimnasio_Alcaraz_Ballerini_Delicia.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gimnasio_Alcaraz_Ballerini_Delicia.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UsuarioService _usuarioService;

        public CuentaController(
            UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
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

            if (usuario == null)
            {
                ViewBag.Mensaje =
                    "Email o password incorrectos.";

                return View();
            }

            if (!usuario.Estado)
            {
                ViewBag.Mensaje =
                    "El usuario se encuentra dado de baja.";

                return View();
            }

            if (usuario.Password != password)
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

            var principal =
                new ClaimsPrincipal(identidad);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                "Login",
                "Cuenta"
            );
        }

        [HttpGet]
        public IActionResult Denegado()
        {
            return View();
        }
    }
}