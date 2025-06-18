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

        public AleaController(EmployeService employeService,AleaService aleaService)
        {
            _employeService = employeService;
            _aleaService = aleaService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Salaire()
        {
            List<Employe> employes = await _employeService.GetAllEmployes(); 


            return View("Salaire",employes);
        }

        public async Task<IActionResult> Generer(string employe, DateOnly? startDate, DateOnly? finDate, float salaire)
        {
            List<SalaryAssignment> assign = await _aleaService.GetRandomSalary(employe, startDate, finDate, salaire);

            foreach (var asm in assign)
            {
                Console.WriteLine($"Employe: {asm.Employee}");
                Console.WriteLine($"Base: {asm.Base}");
                Console.WriteLine($"Salary Structure: {asm.SalaryStructure}");
                Console.WriteLine($"Currency: {asm.Currency}");
                Console.WriteLine($"From Date: {asm.FromDate}");
                Console.WriteLine($"Company: {asm.Company}");
            }

            List<Employe> employes = await _employeService.GetAllEmployes();
            return View("Salaire", employes);
        }

    }
}