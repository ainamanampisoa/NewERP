using Microsoft.AspNetCore.Mvc;
using NewERP.Services;

namespace NewERP.Controllers
{
    public class EmployeController : Controller
    {
        private readonly EmployeService _employeService;
        private readonly DepartmentService _departmentService;

        public EmployeController(EmployeService employeService, DepartmentService departmentService)
        {
            _employeService = employeService;
            _departmentService = departmentService;
        }

        [HttpGet]
        public IActionResult UpdateView()
        {
            return View("Update");
        }

        [HttpGet]
        public async Task<IActionResult> Liste()
        {
            var employes = await _employeService.GetAllEmployes();
            var departments = await _departmentService.GetAllDepartments();

            // Utilise ViewBag pour transmettre les départements à la vue
            ViewBag.Departments = departments;

            return View(employes); // Le modèle principal reste la liste des employés
        }

        [HttpGet]
        public async Task<IActionResult> Filtre(DateTime? datej1, DateTime? datej2, string employee_name, string department, string employmentType, string statut)
        {
            var employes = await _employeService.FiltrerEmployes(datej1, datej2, employee_name, department, employmentType, statut);
            var departments = await _departmentService.GetAllDepartments();

            //Raha ohatra tiana ijanona eo amle formulaire le valeur anle input ao arinan'ny validation
            ViewBag.Departments = departments;
            ViewBag.DateJ1 = datej1?.ToString("yyyy-MM-dd");
            ViewBag.DateJ2 = datej2?.ToString("yyyy-MM-dd");
            ViewBag.EmployeeName = employee_name;
            ViewBag.Department = department;
            ViewBag.EmploymentType = employmentType;
            ViewBag.Statut = statut;

            return View("Liste", employes);
        }

        public async Task<IActionResult> Fiche(string id)
        {
            var fiche = await _employeService.GetFicheEmployeParId(id);
            if (fiche == null)
                return NotFound();

            return View("Fiche", fiche); // Crée une vue Fiche.cshtml pour afficher les infos
        }

    }
}
