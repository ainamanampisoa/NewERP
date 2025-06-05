using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NewERP.Models;
using NewERP.Helpers;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using System;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace NewERP.Services
{
    public class StatistiqueService
    {
        private readonly HttpClient _httpClient;
        private readonly SalaireService _SalaireService;

        public StatistiqueService(HttpClient httpClient, SalaireService SalaireService)
        {
            _httpClient = httpClient;
            _SalaireService = SalaireService;
        }

        private string GetNomMois(int numeroMois)
        {
            var noms = new Dictionary<int, string>
            {
                { 1, "Janvier" },
                { 2, "Février" },
                { 3, "Mars" },
                { 4, "Avril" },
                { 5, "Mai" },
                { 6, "Juin" },
                { 7, "Juillet" },
                { 8, "Août" },
                { 9, "Septembre" },
                { 10, "Octobre" },
                { 11, "Novembre" },
                { 12, "Décembre" }
            };

            return noms.TryGetValue(numeroMois, out string nom) ? nom : $"Mois {numeroMois}";
        }

        public async Task<AnnualSalaryTotals> GetMonthlySalaryTotals(int annee)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            // 🔽 Appel direct à la méthode interne
            var names = await _SalaireService.GetSalarySlipNames();

            // 🔽 Appel direct à la méthode interne
            var allSalaries = await _SalaireService.GetSalarySlipsDetails(names, annee: annee);
            
            if (!allSalaries.Any())
            {
                return new AnnualSalaryTotals
                {
                    Annee = annee,
                    TotauxMensuels = new List<MonthlySalaryTotals>(),
                    TotalAnnuelGrossPay = 0,
                    TotalAnnuelNetPay = 0,
                    TotalAnnuelDeduction = 0
                };
            }

            // Grouper par mois et calculer les totaux
            var monthlyGroups = allSalaries
                .GroupBy(s => s.StartDate.Month)
                .OrderBy(g => g.Key)
                .ToList();

            var totauxMensuels = new List<MonthlySalaryTotals>();

            foreach (var monthGroup in monthlyGroups)
            {
                var mois = monthGroup.Key;
                var salariesDuMois = monthGroup.ToList();

                // Calculer les totaux pour ce mois
                var totalGrossPay = salariesDuMois.Sum(s => s.GrossPay);
                var totalNetPay = salariesDuMois.Sum(s => s.NetPay);
                var totalDeduction = salariesDuMois.Sum(s => s.TotalDeduction);

                // Calculer les totaux par composant d'earnings
                var totalEarningsByComponent = salariesDuMois
                    .Where(s => s.Earnings != null)
                    .SelectMany(s => s.Earnings)
                    .GroupBy(e => e.SalaryComponent)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(e => e.Amount)
                    );

                // Calculer les totaux par composant de déductions
                var totalDeductionsByComponent = salariesDuMois
                    .Where(s => s.Deductions != null)
                    .SelectMany(s => s.Deductions)
                    .GroupBy(d => d.SalaryComponent)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(d => d.Amount)
                    );

                totauxMensuels.Add(new MonthlySalaryTotals
                {
                    Mois = mois,
                    NomMois = GetNomMois(mois),
                    TotalGrossPay = totalGrossPay,
                    TotalNetPay = totalNetPay,
                    TotalDeduction = totalDeduction,
                    TotalEarningsByComponent = totalEarningsByComponent,
                    TotalDeductionsByComponent = totalDeductionsByComponent,
                    NombreBulletins = salariesDuMois.Count()
                });
            }

            // Ajouter les mois manquants avec des totaux à zéro
            for (int i = 1; i <= 12; i++)
            {
                if (!totauxMensuels.Any(t => t.Mois == i))
                {
                    totauxMensuels.Add(new MonthlySalaryTotals
                    {
                        Mois = i,
                        NomMois = GetNomMois(i),
                        TotalGrossPay = 0,
                        TotalNetPay = 0,
                        TotalDeduction = 0,
                        TotalEarningsByComponent = new Dictionary<string, decimal>(),
                        TotalDeductionsByComponent = new Dictionary<string, decimal>(),
                        NombreBulletins = 0
                    });
                }
            }

            // Trier par mois
            totauxMensuels = totauxMensuels.OrderBy(t => t.Mois).ToList();

            return new AnnualSalaryTotals
            {
                Annee = annee,
                TotauxMensuels = totauxMensuels,
                TotalAnnuelGrossPay = totauxMensuels.Sum(t => t.TotalGrossPay),
                TotalAnnuelNetPay = totauxMensuels.Sum(t => t.TotalNetPay),
                TotalAnnuelDeduction = totauxMensuels.Sum(t => t.TotalDeduction)
            };
        }
    }
}