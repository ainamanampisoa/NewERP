using Microsoft.AspNetCore.Mvc;
using NewERP.Services;
using NewERP.Models;
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
        public async Task<IActionResult> Liste(int page = 1, int pageSize = 3)
        {
            // Récupère la liste complète des employés
            var employes = await _employeService.GetAllEmployes();

            // Récupère les listes annexes
            var departments = await _departmentService.GetAllDepartments();
            var genders = await _genderService.GetAllGenders();

            // Pagination
            var totalItems = employes.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginatedEmployes = employes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Passage des données à la vue via ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            ViewBag.Departments = departments;
            ViewBag.Genders = genders;

            // Envoi à la vue la liste paginée d’employés
            return View(paginatedEmployes);
        }

        [HttpGet]
        public async Task<IActionResult> Filtre(
            DateTime? datej1,
            DateTime? datej2,
            string employee_name,
            string department,
            string employmentType,
            string statut,
            string gender,
            int page = 1,
            int pageSize = 3)
        {
            var employes = await _employeService.FiltrerEmployes(datej1, datej2, employee_name, department, employmentType, statut, gender);
            var departments = await _departmentService.GetAllDepartments();
            var genders = await _genderService.GetAllGenders();

            // Pagination
            var totalItems = employes.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginatedEmployes = employes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Données pour garder les filtres
            ViewBag.DateJ1 = datej1?.ToString("yyyy-MM-dd");
            ViewBag.DateJ2 = datej2?.ToString("yyyy-MM-dd");
            ViewBag.EmployeeName = employee_name;
            ViewBag.Department = department;
            ViewBag.Gender = gender;
            ViewBag.EmploymentType = employmentType;
            ViewBag.Statut = statut;

            // Listes annexes et pagination
            ViewBag.Departments = departments;
            ViewBag.Genders = genders;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View("Liste", paginatedEmployes);
        }


        [HttpGet]
        public async Task<IActionResult> Fiche(string id, int page = 1, int pageSize = 3)
        {
            var fiche = await _employeService.GetFicheEmployeParId(id);
            var fichepaie = await _salaireService.GetSalarySlipsParEmployeId(id);

            if (fiche == null)
                return NotFound();

            // Pagination
            var totalItems = fichepaie.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginatedFichePaie = fichepaie
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.FichePaie = paginatedFichePaie;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.IdEmploye = id; // Pour garder l’ID lors du changement de page

            return View("Fiche", fiche);
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
        public async Task<IActionResult> Tableau(int? mois, int? annee, int page = 1, int pageSize = 3)
        {
            var names = await _salaireService.GetSalarySlipNames();
            var allBulletins = await _salaireService.GetSalarySlipsDetails(names, mois, annee);

            // Convertir en liste pour éviter multiples évaluations
            var bulletinsList = allBulletins.ToList();

            // Calcul des totaux sur la liste complète
            decimal totalEarnings = bulletinsList.Sum(slip => slip.GrossPay);
            decimal totalDeductions = bulletinsList.Sum(slip => slip.TotalDeduction);
            decimal totalNetPay = bulletinsList.Sum(slip => slip.NetPay);

            var totalEarningsByComponent = bulletinsList
                    .SelectMany(s => s.Earnings)
                    .GroupBy(e => e.SalaryComponent)
                    .ToDictionary(
                        g => g.Key,
                        g => (decimal)g.Sum(e => e.Amount)
                    );

            // Récupérer tous les composants uniques d'earnings pour les colonnes
            var allEarningComponents = bulletinsList
                .SelectMany(s => s.Earnings)
                .Select(e => e.SalaryComponent)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.TotalEarnings = totalEarnings;
            ViewBag.TotalDeductions = totalDeductions;
            ViewBag.TotalNetPay = totalNetPay;
            ViewBag.TotalByComponent = totalEarningsByComponent;
            ViewBag.EarningComponents = allEarningComponents; // Nouveau

            // Pagination
            int totalItems = bulletinsList.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            page = Math.Max(1, Math.Min(page, totalPages));

            var paginatedBulletins = bulletinsList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Mois = mois;
            ViewBag.Annee = annee;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;

            return View("Tableau", paginatedBulletins);
        }

    }
}
