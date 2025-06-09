using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NewERP.Services;
using NewERP.Models;
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

        public async Task<IActionResult> Upload(IFormFile file1, IFormFile file2, IFormFile file3)
        {
            try
            {
                if (file1 == null && file2 == null && file3 == null)
                {
                    Console.WriteLine("Tsis oooo !!!!");
                    return View();
                }

                Console.WriteLine("Ato oooo !!!!");

                string tempPath1 = null, tempPath2 = null, tempPath3 = null;

                if (file1 != null)
                {
                    tempPath1 = Path.GetTempFileName();
                    using (var stream = new FileStream(tempPath1, FileMode.Create))
                    {
                        await file1.CopyToAsync(stream);
                    }

                    string[] lines = await System.IO.File.ReadAllLinesAsync(tempPath1);
                    foreach (string line in lines)
                    {
                        Console.WriteLine("file1 " + line);
                    }
                }

                if (file2 != null)
                {
                    tempPath2 = Path.GetTempFileName();
                    using (var stream = new FileStream(tempPath2, FileMode.Create))
                    {
                        await file2.CopyToAsync(stream);
                    }
                }

                if (file3 != null)
                {
                    tempPath3 = Path.GetTempFileName();
                    using (var stream = new FileStream(tempPath3, FileMode.Create))
                    {
                        await file3.CopyToAsync(stream);
                    }

                    string[] lines = await System.IO.File.ReadAllLinesAsync(tempPath3);
                    foreach (string line in lines)
                    {
                        Console.WriteLine("file3 " + line);
                    }
                }

                List<string> result = await _dataService.ImportData(tempPath1, tempPath2, tempPath3);

                if (result.Count == 0)
                {
                    ImportResult importResult = await _dataService.ImportDataCsv(tempPath1, tempPath2, tempPath3);

                    if (importResult != null)
                    {
                        if (importResult.Success || importResult.Message.Contains("succès"))
                        {
                            ViewBag.Success = importResult.Message;
                            ViewBag.ImportDetails = importResult.Details;
                        }
                        else
                        {
                            ViewBag.Error = importResult.Message;
                            ViewBag.ErrorType = importResult.Type;
                            ViewBag.Advice = importResult.Advice;
                        }
                    }

                }else{
                    ViewBag.Result=result;
                }

            }
            catch (Exception ex)
            {
                // Afficher le message d'erreur et le détail technique
                ViewBag.Error = "Une erreur est survenue lors de l'importation.";
                ViewBag.ErrorType = ex.GetType().ToString();
                ViewBag.Advice = ex.Message;

                // Pour plus de détails (optionnel)
                ViewBag.StackTrace = ex.StackTrace;
            }

            return View("Index");
        }




        [HttpPost]
        // [ValidateAntiForgeryToken]
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

            return View("Index");
        }

    }
}
