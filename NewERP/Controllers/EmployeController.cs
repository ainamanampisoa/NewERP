using Microsoft.AspNetCore.Mvc;
using NewERP.Services;

namespace NewERP.Controllers
{
    public class EmployeController : Controller
    {
        private readonly EmployeService _employeService;

        public EmployeController(EmployeService employeService)
        {
            _employeService = employeService;
        }

        [HttpGet]
        public async Task<IActionResult> Liste()
        {
            var employes = await _employeService.GetAllEmployes();
            return View(employes);
        }
    }
}
