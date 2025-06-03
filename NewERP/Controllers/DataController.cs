using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NewERP.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NewERP.Controllers
{
    public class DataController : Controller
    {
        private readonly ILogger<DataController> _logger;
        private readonly DataService _dataService;

        public DataController(ILogger<DataController> logger, DataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset()
        {
            try
            {
                string result = await _dataService.ResetDataAsync();
                TempData["Message"] = "Réinitialisation réussie";
                Console.WriteLine(result); // Affiche le résultat dans la console
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la réinitialisation des données.");
                TempData["Erreur"] = "Erreur lors de la reinitialisation";
                Console.WriteLine(ex.Message); // Affiche l'erreur dans la console
            }

            return RedirectToAction("Index");
        }

    }
}
