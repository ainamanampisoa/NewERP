using NewERP.Models;
using NewERP.Helpers;
using Newtonsoft.Json;
using System.Text;


namespace NewERP.Services
{
    public class AleaService
    {
        private readonly HttpClient _httpClient;
        private readonly SalaireService _salaireService;

        public AleaService(HttpClient httpClient, SalaireService salaireService)
        {
            _httpClient = httpClient;
            _salaireService = salaireService;
        }

        public async Task<SalaryAssignment> GetAssignmentEmploye(string employe)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);
            
            string filters = $"[[\"employee\", \"=\", \"{employe}\"]]";
            string fields = "[\"name\", \"employee\", \"salary_structure\", \"base\", \"currency\", \"company\", \"from_date\"]";
            string orderBy = "from_date desc";

            string url = $"http://erpnext.localhost:8000/api/resource/Salary Structure Assignment" +
                        $"?fields={Uri.EscapeDataString(fields)}" +
                        $"&filters={Uri.EscapeDataString(filters)}" +
                        $"&order_by={Uri.EscapeDataString(orderBy)}" +
                        $"&limit_page_length=1";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erreur lors de l'appel API : {response.StatusCode} - {errorContent}");
            }

            string json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonConvert.DeserializeObject<ApiListResponse<SalaryAssignment>>(json);

            if (apiResponse?.Data == null || !apiResponse.Data.Any())
            {
                throw new Exception($"Aucune structure salariale trouvée pour l'employé {employe}");
            }

            return apiResponse.Data.First();
        }


        public async Task<SalaryInfo> ToSalaryInfo(string employe, SalaryAssignment assignments, float salaire)
        {

            var assignment = assignments;
            float salaireBase = 0;
            if (salaire > 0)
            {
                salaireBase = salaire;
                Console.WriteLine($"Salaire: {salaireBase}");

                if (assignments == null)
                {
                    SalaryInfo info = await createAssignmentByStructure(employe, salaire);
                    return info;
                }
            }
            else
            {
                salaireBase = assignment.Base;
            }

            return new SalaryInfo
            {
                SalaireBase = salaireBase,
                Structure = assignment.SalaryStructure,
                EmployeeName = assignment.Company,
                Company = assignment.Company,
                Currency = assignment.Currency
            };
        }

        public async Task CreerSalaryStructureAssignment(string employe, DateOnly currentDate, SalaryInfo salaryInfo)
        {
            var newAssignment = new
            {
                employee = employe,
                salary_structure = salaryInfo.Structure,
                from_date = currentDate.ToString("yyyy-MM-dd"),
                company = salaryInfo.Company,
                currency = salaryInfo.Currency,
                @base = salaryInfo.SalaireBase,
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
        public async Task CreerSalarySlip(string employe, DateOnly currentDate, SalaryInfo salaryInfo)
        {
            var newAssignment = new
            {
                employee = employe,
                company = salaryInfo.Company,

                salary_structure = salaryInfo.Structure,
                @base = salaryInfo.SalaireBase,

                start_date = new DateTime(currentDate.Year, currentDate.Month, 1).ToString("yyyy-MM-dd"),
                end_date = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)).ToString("yyyy-MM-dd"),
                posting_date = DateTime.Now.ToString("yyyy-MM-dd"),

                payroll_frequency = "Monthly",
                currency = salaryInfo.Currency,
                docstatus = 1,

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
                Console.WriteLine($"Salary slip inséré avec succès pour {salaryInfo.EmployeeName} - {currentDate:yyyy-MM-dd}");
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

        public async Task<SalaryInfo> createAssignmentByStructure(string employeeT, float salaire)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string fields = "[\"name\", \"company\", \"currency\"]";

            string url = $"http://erpnext.localhost:8000/api/resource/Salary Structure?fields={Uri.EscapeDataString(fields)}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"❌ Impossible de récupérer la Salary Structure : {response.ReasonPhrase}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ApiResponse<List<SalaryStructure>>>(json);
            SalaryStructure data = result?.Data?.FirstOrDefault();

            Console.WriteLine("URL appelée Salary Structure : " + url);

            if (data == null)
            {
                throw new Exception("❌ Aucune structure salariale trouvée.");
            }

            Console.WriteLine($"Indret iany ny donnee: {data.Name}, {data.Company}, {data.Currency}, {salaire}, {employeeT}");

            return new SalaryInfo
            {
                Structure = data.Name,
                Company = data.Company,
                Currency = data.Currency,
                SalaireBase = salaire,
                EmployeeName = employeeT
            };
        }

        public async Task CreerAssignmentsEtSlips(string employe, DateOnly? startDate, DateOnly? finDate, SalaryInfo salaryInfo)
        {
            for (DateOnly currentDate = startDate.Value; currentDate <= finDate; currentDate = currentDate.AddMonths(1))
            {
                bool existeDeja = await VerifierExistenceSalaire(employe, currentDate);
                if (!existeDeja)
                {
                    await CreerSalaryStructureAssignment(employe, currentDate, salaryInfo);
                    await CreerSalarySlip(employe, currentDate, salaryInfo);
                }
                else
                {
                    Console.WriteLine($"Salary assignment déjà existant pour {currentDate:yyyy-MM-dd}, ignoré.");
                }
            }
        }

        public async Task GetRandomSalary(string employe, DateOnly? startDate, DateOnly? finDate, float salaire)
        {
            var assignments = await GetAssignmentEmploye(employe);
            SalaryInfo info = await ToSalaryInfo(employe, assignments, salaire);

            await CreerAssignmentsEtSlips(employe, startDate, finDate, info);
        }

        //ALEA2

        public async Task<List<SalarySlip>> SalarySlipsDetails()
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);
            List<string> names = await _salaireService.GetSalarySlipNames();

            Console.WriteLine("Name Salary Slip : " + names.Count);

            var salarySlips = new List<SalarySlip>();

            foreach (var name in names)
            {
                string url = $"http://erpnext.localhost:8000/api/resource/Salary Slip/{name}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) continue;

                string json = await response.Content.ReadAsStringAsync();

                var salarySlipResponse = JsonConvert.DeserializeObject<ApiResponse<SalarySlip>>(json);
                var slip = salarySlipResponse?.Data;
                if (slip == null) continue;
                salarySlips.Add(slip);
            }

            return salarySlips;
        }

        public async Task<SalarySlip> SalarySlipsDetailsByName(string name)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string url = $"http://erpnext.localhost:8000/api/resource/Salary Slip/{name}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) ;

            string json = await response.Content.ReadAsStringAsync();

            var salarySlipResponse = JsonConvert.DeserializeObject<ApiResponse<SalarySlip>>(json);
            SalarySlip slip = salarySlipResponse?.Data;

            return slip;
        }

        public async Task<SalaryAssignment> GetAssignmentBySlip(string employeeT, DateOnly dateD)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string filters = $"[[\"employee\", \"=\", \"{employeeT}\"], [\"from_date\", \"=\", \"{dateD:yyyy-MM-dd}\"]]";
            string fields = "[\"name\", \"employee\", \"salary_structure\", \"base\", \"company\", \"currency\", \"from_date\"]";

            string url = $"http://erpnext.localhost:8000/api/resource/Salary Structure Assignment?filters={Uri.EscapeDataString(filters)}&fields={Uri.EscapeDataString(fields)}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erreur récupération assignment : {response.ReasonPhrase}");

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<SalaryAssignment>>>(json);

            return apiResponse?.Data?.FirstOrDefault();
        }


        public async Task<bool> cancelSalary(string slipName)
        {
            string cancelUrl = $"http://erpnext.localhost:8000/api/resource/Salary Slip/{slipName}";
            var cancelData = new { docstatus = 2 };
            var cancelContent = new StringContent(JsonConvert.SerializeObject(cancelData), Encoding.UTF8, "application/json");

            HttpResponseMessage cancelResponse = await _httpClient.PutAsync(cancelUrl, cancelContent);

            if (!cancelResponse.IsSuccessStatusCode)
            {
                string cancelError = await cancelResponse.Content.ReadAsStringAsync();
                Console.WriteLine($" Échec de l'annulation de {slipName} : {cancelError}");
                return false;
            }

            Console.WriteLine($"Salary Slip {slipName} annulé avec succès");
            return true;
        }

        public async Task<bool> cancelAssignment(string assignmentName)
        {
            string cancelUrl = $"http://erpnext.localhost:8000/api/resource/Salary Structure Assignment/{assignmentName}";
            var cancelData = new { docstatus = 2 };
            var cancelContent = new StringContent(JsonConvert.SerializeObject(cancelData), Encoding.UTF8, "application/json");

            HttpResponseMessage cancelResponse = await _httpClient.PutAsync(cancelUrl, cancelContent);

            if (!cancelResponse.IsSuccessStatusCode)
            {
                string cancelError = await cancelResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Échec de l'annulation de l'Assignment {assignmentName} : {cancelError}");
                return false;
            }

            Console.WriteLine($"Assignment {assignmentName} annulé avec succès");
            return true;
        }

        public async Task<bool> DeleteSalarySlip(string slipName)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string url = $"http://erpnext.localhost:8000/api/resource/Salary Slip/{slipName}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Échec de la suppression du Salary Slip {slipName} : {error}");
                return false;
            }

            Console.WriteLine($"Salary Slip {slipName} supprimé avec succès");
            return true;
        }

        public async Task<bool> DeleteSalaryStructure(string assignmentName)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string url = $"http://erpnext.localhost:8000/api/resource/Salary Structure Assignment/{assignmentName}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($" Échec de la suppression de l'Assignment {assignmentName} : {error}");
                return false;
            }

            Console.WriteLine($"Assignment {assignmentName} supprimé avec succès");
            return true;
        }

        public async Task<SalaryInfo> ExtraireSalaryInfoPorcent(SalaryAssignment assi, float pourcent, int calcul)
        {
            if (assi == null)
            {
                return new SalaryInfo();
            }

            float newAmount = 0;

            if (calcul == 1) // Addition
            {
                newAmount = (float)assi.Base + ((float)assi.Base * pourcent / 100);
                Console.WriteLine($"ADDITION - Initial: {assi.Base} + {pourcent}% ----> Final: {newAmount}");
            }
            else if (calcul == 0) // Soustraction
            {
                newAmount = (float)assi.Base - ((float)assi.Base * pourcent / 100);
                Console.WriteLine($"SOUSTRACTION - Initial: {assi.Base} - {pourcent}% ----> Final: {newAmount}");
            }
            else
            {
                newAmount = (float)assi.Base;
                Console.WriteLine($"AUCUNE MODIFICATION - Initial: {assi.Base} ----> Final: {newAmount}");
            }

            return new SalaryInfo
            {
                SalaireBase = newAmount,
                Structure = assi.SalaryStructure,
                EmployeeName = assi.name,
                Company = assi.Company,
                Currency = assi.Currency
            };
        }


        public async Task<bool> newSalaire(string nameSlip, float pourcent, int calcul)
        {

            Console.WriteLine($"Appel newSalaire - nameSlip: {nameSlip}, pourcent: {pourcent}, calcul: {calcul}");
            var salarySlip = await SalarySlipsDetailsByName(nameSlip);
            if (salarySlip == null) return false;

            string employeeT = salarySlip.Employee;
            var dateDebut = DateOnly.FromDateTime(salarySlip.StartDate);
            var assi = await GetAssignmentBySlip(employeeT, dateDebut);

            if (assi == null) return false;

            bool canceledSlip = await cancelSalary(nameSlip);
            bool canceledAssign = await cancelAssignment(assi.name);

            if (canceledSlip && canceledAssign)
            {
                await DeleteSalarySlip(nameSlip);
                await DeleteSalaryStructure(assi.name);

                var info = await ExtraireSalaryInfoPorcent(assi, pourcent, calcul);

                await CreerSalaryStructureAssignment(employeeT, dateDebut, info);
                await CreerSalarySlip(employeeT, dateDebut, info);

                return true;
            }

            return false;
        }

        public async Task<bool> update(string component, int signe, decimal montant, float pourcent, int calcul)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);
            var allSalaries = await SalarySlipsDetails();

            var earningsMatch = allSalaries
                .SelectMany(slip => slip.Earnings.Select(e => new { Slip = slip, Earning = e }))
                .Where(x => x.Earning.SalaryComponent == component)
                .Where(x => (signe == 0 && x.Earning.Amount < montant) || (signe == 1 && x.Earning.Amount > montant))
                .ToList();

            var deductionsMatch = allSalaries
                .SelectMany(slip => slip.Deductions.Select(d => new { Slip = slip, Deduction = d }))
                .Where(x => x.Deduction.SalaryComponent == component)
                .Where(x => (signe == 0 && x.Deduction.Amount < montant) || (signe == 1 && x.Deduction.Amount > montant))
                .ToList();

            if (!earningsMatch.Any() && !deductionsMatch.Any())
            Console.WriteLine("Aucun élément trouvé correspondant aux critères.");

            Console.WriteLine(" GAIN(S):");
            foreach (var match in earningsMatch)
            {
                Console.WriteLine($"Employee: {match.Slip.Employee} | Amount: {match.Earning.Amount} | Component: {match.Earning.SalaryComponent}");
                await newSalaire(match.Slip.Name, pourcent, calcul);
            }

            Console.WriteLine(" DÉDUCTION(S) :");
            foreach (var match in deductionsMatch)
            {
                Console.WriteLine($"Employee: {match.Slip.Employee} | Amount: {match.Deduction.Amount} | Component: {match.Deduction.SalaryComponent}");
                await newSalaire(match.Slip.Name, pourcent, calcul);
            }

            return true;
        }

    }   
}