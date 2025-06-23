using Microsoft.AspNetCore.Mvc;
using NewERP.Services;
using System.Threading.Tasks;

namespace NewERP.Controllers
{
    public class ComponentController : Controller
    {
        private readonly ComponentService _componentService;

        public ComponentController(ComponentService componentService)
        {
            _componentService = componentService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string abbr, string type)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(abbr) || string.IsNullOrWhiteSpace(type))
            {
                TempData["Erreur"] = "Tous les champs obligatoires doivent être remplis.";
                return RedirectToAction("Index", "Component");
            }

            bool success = await _componentService.CreateSalaryComponent(name, abbr, type);

            if (success)
            {
                TempData["Succes"] = "Composant de salaire créé avec succès.";
            }
            else
            {
                TempData["Erreur"] = "Erreur lors de la création du composant.";
            }

            return RedirectToAction("Index", "Component");
        }
    }
}
