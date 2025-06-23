using Microsoft.AspNetCore.Mvc;
using NewERP.Services;
using NewERP.Models;
using Microsoft.AspNetCore.Authorization;

namespace NewERP.Controllers
{
    [Authorize]
    public class AleaController : Controller
    {
        private readonly EmployeService _employeService;
        private readonly AleaService _aleaService;

        private readonly ComponentService _componentService;

        public AleaController(EmployeService employeService, AleaService aleaService, ComponentService componentService)
        {
            _employeService = employeService;
            _aleaService = aleaService;
            _componentService = componentService;
        }

        [HttpGet]
        public async Task<IActionResult> Salaire()
        {
            List<Employe> employes = await _employeService.GetAllEmployes();
            var components = await _componentService.GetAllSalaryComponents();
            
            Console.WriteLine($"Nombre de composants récupérés: {components?.Count ?? 0}");
            
            ViewBag.Components = components;
            return View("Salaire", employes);
        }

        public async Task<IActionResult> Generer(string employe, DateOnly? startDate, DateOnly? finDate, float salaire)
        {
            await _aleaService.GetRandomSalary(employe, startDate, finDate, salaire);
            
            ViewBag.Succes = "✔ Salaire ajouté avec succès !";

            List<Employe> employes = await _employeService.GetAllEmployes();
//mba ampiverina anle component
            var components = await _componentService.GetAllSalaryComponents();
            ViewBag.Components = components;

            return View("Salaire", employes);
        }

        public async Task<IActionResult> Update(string component, int signe, decimal montant, float pourcent, int calcul)
        {
            await _aleaService.update(component, signe, montant, pourcent, calcul);

            List<Employe> employes = await _employeService.GetAllEmployes();

            ViewBag.ConfirmationMessage = "✔ Mise à jour effectuée avec succès !";

//mba ampiverina anle composant eo am page
            var components = await _componentService.GetAllSalaryComponents();
            ViewBag.Components = components;

            return View("Salaire", employes);
        }

    }
}