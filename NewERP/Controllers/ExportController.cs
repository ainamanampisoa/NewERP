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
    public class ExportController : Controller
    {
        private readonly SalaireService _salaireService;

        public ExportController(SalaireService salaireService)
        {
            _salaireService = salaireService;
        }

        [HttpGet]
        public async Task<IActionResult> Csv(string employeeId)
        {
            try
            {
                var slips = await _salaireService.GetSalarySlipsParEmployeId(employeeId);
                Console.WriteLine(slips);
                
                // Nettoyer l'employeeId pour éviter les caractères problématiques
                string cleanEmployeeId = employeeId
                    .Replace("/", "_")
                    .Replace("\\", "_")
                    .Replace(" ", "_")
                    .Replace(":", "_")
                    .Replace("*", "_")
                    .Replace("?", "_")
                    .Replace("\"", "_")
                    .Replace("<", "_")
                    .Replace(">", "_")
                    .Replace("|", "_");
                
                string filename = $"SalarySlips_{cleanEmployeeId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                // Utiliser des slashes forward pour Linux/Unix (pas de backslashes)
                string directoryPath = "/home/aina/Documents/Evaluation/CSV";
                string filepath = Path.Combine(directoryPath, filename);

                // Créer le répertoire s'il n'existe pas
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Console.WriteLine($"Répertoire créé : {directoryPath}");
                }

                ExportService.ExportToCsv(slips, filepath);

                if (System.IO.File.Exists(filepath))
                {
                    Console.WriteLine($"Fichier CSV généré avec succès : {filepath}");
                    return Ok($"Fichier sauvegardé dans : {filepath}");
                }
                else
                {
                    return BadRequest("Erreur lors de la création du fichier");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur : {ex.Message}");
            }
        }
    }
}