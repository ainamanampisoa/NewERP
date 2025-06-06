using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewERP.Models;
using NewERP.Services;
using Microsoft.AspNetCore.Authorization;

namespace NewERP.Controllers
{
    [Authorize]
    public class StatistiqueController : Controller
    {
        private readonly SalaireService _salaireService;
        private readonly StatistiqueService _statistiqueService;

        public StatistiqueController(SalaireService salaireService, StatistiqueService statistiqueService)
        {
            _salaireService = salaireService;
            _statistiqueService = statistiqueService;
        }

        [HttpGet]
        public async Task<IActionResult> Tableau(int annee, int page = 1, int pageSize = 3)
        {
            try
            {
                if(annee == 0){
                    annee = 2025;
                }
                var totauxAnnuels = await _statistiqueService.GetMonthlySalaryTotals(annee);

                // Pagination
                var totalItems = totauxAnnuels.TotauxMensuels.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var paginatedMensuels = totauxAnnuels.TotauxMensuels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                Console.WriteLine($"=== TOTAUX POUR L'ANNÉE {totauxAnnuels.Annee} ===");
                Console.WriteLine($"Total annuel Gross Pay: {totauxAnnuels.TotalAnnuelGrossPay:C}");
                Console.WriteLine($"Total annuel Net Pay: {totauxAnnuels.TotalAnnuelNetPay:C}");
                Console.WriteLine($"Total annuel Déductions: {totauxAnnuels.TotalAnnuelDeduction:C}");
                Console.WriteLine();

                foreach (var mois in totauxAnnuels.TotauxMensuels)
                {
                    Console.WriteLine($"=== {mois.NomMois.ToUpper()} ===");
                    Console.WriteLine($"Nombre de bulletins: {mois.NombreBulletins}");
                    Console.WriteLine($"Gross Pay: {mois.TotalGrossPay:C}");
                    Console.WriteLine($"Net Pay: {mois.TotalNetPay:C}");
                    Console.WriteLine($"Déductions: {mois.TotalDeduction:C}");

                    if (mois.TotalEarningsByComponent.Any())
                    {
                        Console.WriteLine("Détail des gains par composant:");
                        foreach (var earning in mois.TotalEarningsByComponent)
                        {
                            Console.WriteLine($"  - {earning.Key}: {earning.Value:C}");
                        }
                    }

                    if (mois.TotalDeductionsByComponent.Any())
                    {
                        Console.WriteLine("Détail des déductions par composant:");
                        foreach (var deduction in mois.TotalDeductionsByComponent)
                        {
                            Console.WriteLine($"  - {deduction.Key}: {deduction.Value:C}");
                        }
                    }

                    Console.WriteLine();
                }

                ViewBag.Annee = annee;
                ViewBag.TotauxAnnuels = totauxAnnuels;
                ViewBag.Mensuels = paginatedMensuels;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;

                return View("Tableau");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Data.Contains("ResponseContent"))
                {
                    var errorContent = ex.Data["ResponseContent"] as string;
                    ViewBag.Erreur = $"Erreur HTTP : {ex.Message}\nDétails : {errorContent}";
                }
                else
                {
                    ViewBag.Erreur = $"Erreur HTTP : {ex.Message}";
                }
                return View("Tableau", new List<SalarySlip>());
            }
            catch (System.Exception ex)
            {
                ViewBag.Erreur = $"Erreur interne : {ex.Message}";
                return View("Tableau", new List<SalarySlip>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? mois, int? annee, int page = 1, int pageSize = 3)
        {
                var names = await _salaireService.GetSalarySlipNames();
                var allBulletins = await _salaireService.GetSalarySlipsDetails(names, mois, annee);

                // Convertir en liste pour éviter multiples évaluations
                var bulletinsList = allBulletins.ToList();

                // Calcul des totaux sur la liste complète
                decimal totalEarnings = bulletinsList.Sum(slip => slip.GrossPay);
                decimal totalDeductions = bulletinsList.Sum(slip => slip.TotalDeduction);
                decimal totalNetPay = bulletinsList.Sum(slip => slip.NetPay);

                ViewBag.TotalEarnings = totalEarnings;
                ViewBag.TotalDeductions = totalDeductions;
                ViewBag.TotalNetPay = totalNetPay;

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

                return View("Details", paginatedBulletins);
        }

        [HttpGet]
        public async Task<IActionResult> Graphe(int annee)
        {
            try
            {
                if(annee == 0){
                    annee = 2025;
                }
                var totauxAnnuels = await _statistiqueService.GetMonthlySalaryTotals(annee);


                Console.WriteLine($"=== TOTAUX POUR L'ANNÉE {totauxAnnuels.Annee} ===");
                Console.WriteLine($"Total annuel Gross Pay: {totauxAnnuels.TotalAnnuelGrossPay:C}");
                Console.WriteLine($"Total annuel Net Pay: {totauxAnnuels.TotalAnnuelNetPay:C}");
                Console.WriteLine($"Total annuel Déductions: {totauxAnnuels.TotalAnnuelDeduction:C}");
                Console.WriteLine();

                foreach (var mois in totauxAnnuels.TotauxMensuels)
                {
                    Console.WriteLine($"=== {mois.NomMois.ToUpper()} ===");
                    Console.WriteLine($"Nombre de bulletins: {mois.NombreBulletins}");
                    Console.WriteLine($"Gross Pay: {mois.TotalGrossPay:C}");
                    Console.WriteLine($"Net Pay: {mois.TotalNetPay:C}");
                    Console.WriteLine($"Déductions: {mois.TotalDeduction:C}");

                    if (mois.TotalEarningsByComponent.Any())
                    {
                        Console.WriteLine("Détail des gains par composant:");
                        foreach (var earning in mois.TotalEarningsByComponent)
                        {
                            Console.WriteLine($"  - {earning.Key}: {earning.Value:C}");
                        }
                    }

                    if (mois.TotalDeductionsByComponent.Any())
                    {
                        Console.WriteLine("Détail des déductions par composant:");
                        foreach (var deduction in mois.TotalDeductionsByComponent)
                        {
                            Console.WriteLine($"  - {deduction.Key}: {deduction.Value:C}");
                        }
                    }

                    Console.WriteLine();
                }

                ViewBag.Annee = annee;
                ViewBag.TotauxAnnuels = totauxAnnuels;

                return View("Graphe");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Data.Contains("ResponseContent"))
                {
                    var errorContent = ex.Data["ResponseContent"] as string;
                    ViewBag.Erreur = $"Erreur HTTP : {ex.Message}\nDétails : {errorContent}";
                }
                else
                {
                    ViewBag.Erreur = $"Erreur HTTP : {ex.Message}";
                }
                return View("Graphe", new List<SalarySlip>());
            }
            catch (System.Exception ex)
            {
                ViewBag.Erreur = $"Erreur interne : {ex.Message}";
                return View("Graphe", new List<SalarySlip>());
            }
        }
    }
}