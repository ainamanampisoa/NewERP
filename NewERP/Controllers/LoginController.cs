using NewERP.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NewERP.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginService _loginService;
        public LoginController(LoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Accueil()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Connexion(string Username, string Password)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe vide.");
                return View("Login");
            }

            Console.WriteLine($"Tentative de connexion de l'utilisateur : {Username}");

            var sid = await _loginService.Login(Username, Password);

            if (sid != null)
            {
                Response.Cookies.Append("sid", sid, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });

                HttpContext.Session.SetString("AuthToken", sid);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                await HttpContext.SignInAsync(
                    "CookieAuth",
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Accueil","Login");
            }

            ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe incorrect.");
            return View("Login");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // 1. Supprimer le cookie d'authentificatikdlon
            await HttpContext.SignOutAsync("CookieAuth");

            // 2. Supprimer le cookie "sid" manuellement ajouté
            Response.Cookies.Delete("sid");

            // 3. Nettoyer complètement la session
            HttpContext.Session.Clear();

            // Redirection vers la page de login
            return RedirectToAction("Login", "Login");
        }
    }
}