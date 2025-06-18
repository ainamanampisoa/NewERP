using NewERP.Models;
using NewERP.Helpers;
using Newtonsoft.Json;
using System.Text;

namespace NewERP.Services
{
    public class AleaService
    {
        private readonly HttpClient _httpClient;

        public AleaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<List<SalaryAssignment>> GetRandomSalary(string employe, DateOnly? startDate, DateOnly? finDate, float salaire)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);
            List<SalaryAssignment> assng = new List<SalaryAssignment>();

            string filters = $"[[\"employee\", \"=\", \"{employe}\"]]";
            string fields = "[\"name\", \"employee\", \"salary_structure\", \"base\", \"currency\", \"company\", \"from_date\"]";

            string url = $"http://erpnext.localhost:8000/api/resource/Salary Structure Assignment?fields={Uri.EscapeDataString(fields)}&filters={Uri.EscapeDataString(filters)}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erreur lors de l'appel API : {response.StatusCode} - {errorContent}");
                }

                string json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<SalaryAssignment>>>(json);

                if (apiResponse?.Data == null || apiResponse.Data.Count == 0)
                {
                    throw new Exception($"Aucune structure salariale trouvée pour l'employé {employe}");
                }

                assng = apiResponse.Data;

                float salaireBase = 0;
                string structure = null;
                string currency = null;
                string company = null;

                if (assng.Count > 0)
                {
                    structure = assng[0].SalaryStructure;
                    currency = assng[0].Currency;
                    company = assng[0].Company;
                }
                Console.WriteLine($"Strucutre: {structure}");
                Console.WriteLine($"Currency: {currency}");
                Console.WriteLine($"Company: {company}");
                Console.WriteLine($"Employee: {employe}");

                if (salaire > 0)
                {
                    salaireBase = salaire;
                    Console.WriteLine($"Salaire: {salaireBase}");
                }
                else
                {
                    SalaryAssignment assignmentReference = null;
                    DateOnly? latestDate = null;

                    for (int i = 0; i < assng.Count; i++)
                    {
                        var a = assng[i];
                        if (a.FromDate.HasValue)
                        {
                            if (latestDate == null || a.FromDate > latestDate)
                            {
                                latestDate = a.FromDate;
                                assignmentReference = a;
                            }
                        }
                    }

                    if (assignmentReference != null)
                    {
                        salaireBase = assignmentReference.Base;
                    }
                    else
                    {
                        throw new Exception($"Impossible de déterminer le salaire de base pour l'employé {employe}");
                    }
                }


                for (DateOnly currentDate = startDate.Value; currentDate <= finDate.Value; currentDate = currentDate.AddMonths(1))
                {
                    bool existeDeja = await VerifierExistenceSalaire(employe, currentDate);
                    if (!existeDeja)
                    {
                        var assignmentModel = new SalaryAssignment
                        {
                            SalaryStructure = structure,
                            Company = company,
                            Currency = currency,
                            Base = salaireBase,
                        };

                        await CreerSalaryStructureAssignment(employe, currentDate, assignmentModel);
                        Console.WriteLine($"Salary assignment créé pour {currentDate:yyyy-MM-dd}");
                        
                        await CreerSalarySlip(employe,currentDate, assignmentModel);
                        Console.WriteLine($"Salary slip créé pour {currentDate:yyyy-MM-dd}");


                    }
                    else
                    {
                        Console.WriteLine($"Salary assignment déjà existant pour {currentDate:yyyy-MM-dd}, ignoré.");
                    }
                    
                }

                return assng;
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"Erreur de réseau lors de l'appel API : {httpEx.Message}", httpEx);
            }
        }

        public async Task CreerSalaryStructureAssignment(string employe, DateOnly currentDate, SalaryAssignment salaryInfo)
        {
            var newAssignment = new
            {
                employee = employe,
                salary_structure = salaryInfo.SalaryStructure,
                from_date = currentDate.ToString("yyyy-MM-dd"),
                company = salaryInfo.Company,
                currency = salaryInfo.Currency,
                @base = salaryInfo.Base,
                docstatus = 1
            };

            string url = "http://erpnext.localhost:8000/api/resource/Salary Structure Assignment";
            var content = new StringContent(JsonConvert.SerializeObject(newAssignment), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Assignment existe déjà pour la date : " + currentDate.ToString("yyyy-MM-dd"));
            }
        }
        public async Task CreerSalarySlip(string employe,DateOnly currentDate, SalaryAssignment salaryInfo)
        {
            var newAssignment = new
            {
                employee = employe,
                company = salaryInfo.Company,
            
                salary_structure = salaryInfo.SalaryStructure,
                @base = salaryInfo.Base,

                start_date = new DateTime(currentDate.Year, currentDate.Month, 1).ToString("yyyy-MM-dd"),
                end_date = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)).ToString("yyyy-MM-dd"),
                posting_date = DateTime.Now.ToString("yyyy-MM-dd"),

                payroll_frequency = "Monthly",
                currency = salaryInfo.Currency,
                status = "Draft",

                payment_days = 30,
                leave_without_pay = 0,
                arrear_amount = 0,
                incentive_amount = 0,
                remarks = "Créé automatiquement via API"
            };

            string url = "http://erpnext.localhost:8000/api/resource/Salary Slip";
            var content = new StringContent(JsonConvert.SerializeObject(newAssignment), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Erreur lors de la création du Salary Slip pour : " + currentDate.ToString("yyyy-MM-dd") + " => " + error);
            }
            else
            {
                Console.WriteLine($"Salary slip inséré avec succès pour {salaryInfo.Employee} - {currentDate:yyyy-MM-dd}");
            }
        }

        private async Task<bool> VerifierExistenceSalaire(string employe, DateOnly date)
        {
            try
            {
                string filters = $"[[\"employee\", \"=\", \"{employe}\"], [\"from_date\", \"=\", \"{date:yyyy-MM-dd}\"]]";
                string fields = "[\"name\"]";

                string url = $"http://erpnext.localhost:8000/api/resource/Salary Structure Assignment?fields={Uri.EscapeDataString(fields)}&filters={Uri.EscapeDataString(filters)}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return false;
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<SalaryAssignment>>>(json);
                    return apiResponse?.Data != null && apiResponse.Data.Count > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la vérification d'existence : {ex.Message}");
                return false;
            }
        }
    }
}