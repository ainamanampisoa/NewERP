using Microsoft.AspNetCore.Mvc;
using NewERP.Services;
using Microsoft.AspNetCore.Authorization;

namespace NewERP.Controllers
{
    [Authorize]
    public class EmployeController : Controller
    {
        private readonly EmployeService _employeService;
        private readonly DepartmentService _departmentService;
        private readonly GenderService _genderService;
        private readonly SalaireService _salaireService;

        public EmployeController(EmployeService employeService, DepartmentService departmentService,GenderService genderService,SalaireService salaireService)
        {
            _employeService = employeService;
            _departmentService = departmentService;
            _genderService = genderService;
            _salaireService = salaireService;
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
            var genders = await _genderService.GetAllGenders();

            // Utilise ViewBag pour transmettre les départements à la vue
            ViewBag.Departments = departments;
            ViewBag.Genders = genders;

            return View(employes); // Le modèle principal reste la liste des employés
        }

        [HttpGet]
        public async Task<IActionResult> Filtre(DateTime? datej1, DateTime? datej2, string employee_name, string department, string employmentType, string statut,string gender)
        {
            var employes = await _employeService.FiltrerEmployes(datej1, datej2, employee_name, department, employmentType, statut,gender);
            var departments = await _departmentService.GetAllDepartments();
            var genders = await _genderService.GetAllGenders();

            //Raha ohatra tiana ijanona eo amle formulaire le valeur anle input ao arinan'ny validation
            ViewBag.Departments = departments;
            ViewBag.Genders = genders;
            ViewBag.DateJ1 = datej1?.ToString("yyyy-MM-dd");
            ViewBag.DateJ2 = datej2?.ToString("yyyy-MM-dd");
            ViewBag.EmployeeName = employee_name;
            ViewBag.Department = department;
            ViewBag.Gender = gender;
            ViewBag.EmploymentType = employmentType;
            ViewBag.Statut = statut;

            return View("Liste", employes);
        }

        [HttpGet]
        public async Task<IActionResult> Fiche(string id)
        {
            var fiche = await _employeService.GetFicheEmployeParId(id);
            var fichepaie = await _salaireService.GetSalarySlipsParEmployeId(id);
            ViewBag.FichePaie=fichepaie;
            if (fiche == null)
                return NotFound();

            return View("Fiche", fiche); //View Fiche , variable fiche
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(string id)
        {
            var slip = await _salaireService.GetSalarySlipParNomAsync(id);
            if (slip == null)
                return NotFound();

            _salaireService.ExporterSalarySlipEnPdf(slip);

            // Redirige vers la fiche ou une page de confirmation
            return RedirectToAction("Fiche", new { id = slip.Employee });
        }

        [HttpGet]
        public async Task<IActionResult> Tableau(int? mois, int? annee)
        {
            List<SalarySlip> bulletins = new List<SalarySlip>();

            if (mois.HasValue && annee.HasValue)
            {
                // Cas normal : mois + année
                bulletins = await _salaireService.GetSalarySlipsParMoisEtAnnee(mois.Value, annee.Value);
            }
            else if (mois.HasValue)
            {
                // Cas spécial : mois seul → on récupère tous les bulletins de ce mois dans n’importe quelle année
                bulletins = await _salaireService.GetSalarySlipsParMoisTousAnnees(mois.Value);
            }

            return View("Tableau", bulletins);
        }


    }
}
