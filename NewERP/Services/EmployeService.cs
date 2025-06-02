using Newtonsoft.Json.Linq;
using NewERP.Models;
using NewERP.Helpers;

namespace NewERP.Services
{
    public class EmployeService
    {
        private readonly HttpClient _httpClient;

        public EmployeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Employe>> GetAllEmployes()
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string url = "http://erpnext.localhost:8000/api/resource/Employee?fields=[\"name\",\"employee_name\",\"date_of_birth\",\"gender\",\"date_of_joining\",\"status\",\"department\"]";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var data = json["data"].ToObject<List<Employe>>();

            return data;
        }

        // public async Task<bool> UpdateEmploye(string id, Employe employe)
        // {
        //     FrappeAuthHelper.AjouterAuthorization(_httpClient);

        //     string url = $"http://erpnext.localhost:8000/api/resource/Employee/{id}";

        //     var content = new StringContent(
        //         Newtonsoft.Json.JsonConvert.SerializeObject(employe),
        //         System.Text.Encoding.UTF8,
        //         "application/json"
        //     );

        //     var response = await _httpClient.PutAsync(url, content);
        //     return response.IsSuccessStatusCode;
        // }

        public async Task<List<Employe>> FiltrerEmployes(DateTime? dateDebut, DateTime? dateFin, string employeeName, string department, string employmentType, string statut,string gender)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            // Construction de la condition dynamique (utilise Frappe ERP filters)
            var filters = new List<object>();

            if (!string.IsNullOrWhiteSpace(employeeName))
                filters.Add(new[] { "employee_name", "like", $"%{employeeName}%" });

            if (!string.IsNullOrWhiteSpace(department))
                filters.Add(new[] { "department", "=", department });
            
            if (!string.IsNullOrWhiteSpace(gender))
                filters.Add(new[] { "gender", "=", gender });

            if (!string.IsNullOrWhiteSpace(employmentType))
                filters.Add(new[] { "employment_type", "=", employmentType });

            if (!string.IsNullOrWhiteSpace(statut))
                filters.Add(new[] { "status", "=", statut });

            if (dateDebut.HasValue)
                filters.Add(new[] { "date_of_joining", ">=", dateDebut.Value.ToString("yyyy-MM-dd") });

            if (dateFin.HasValue)
                filters.Add(new[] { "date_of_joining", "<=", dateFin.Value.ToString("yyyy-MM-dd") });

            string filterJson = Newtonsoft.Json.JsonConvert.SerializeObject(filters);
            string fields = "[\"name\",\"employee_name\",\"date_of_birth\",\"gender\",\"date_of_joining\",\"status\",\"department\"]";
            string filterJsonEncoded = Uri.EscapeDataString(filterJson);
            string fieldsEncoded = Uri.EscapeDataString(fields);

            string url = $"http://erpnext.localhost:8000/api/resource/Employee?fields={fieldsEncoded}&filters={filterJsonEncoded}";
            Console.WriteLine(url);
            // string url = $"http://erpnext.localhost:8000/api/resource/Employee?fields={fields}&filters={filterJson}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var data = json["data"].ToObject<List<Employe>>();
            return data;
        }

        public async Task<Employe> GetFicheEmployeParId(string id)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string fields = "[\"name\",\"employee_name\",\"date_of_birth\",\"gender\",\"date_of_joining\",\"status\",\"department\"]";
            string fieldsEncoded = Uri.EscapeDataString(fields);
         
            string url = $"http://erpnext.localhost:8000/api/resource/Employee/{id}?fields={fieldsEncoded}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var data = json["data"].ToObject<Employe>();
            return data;
        }

        // public async Task DeleteEmployee(string name)
        // {
        //     FrappeAuthHelper.AjouterAuthorization(_httpClient);

        //     string detailUrl = $"http://erpnext.localhost:8000/api/resource/Employee/{name}";

        //     try
        //     {
        //         HttpResponseMessage response = await _httpClient.DeleteAsync(detailUrl);
        //         if (response.IsSuccessStatusCode)
        //         {
        //             Console.WriteLine($"Employé {name} supprimé avec succès.");
        //         }
        //         else
        //         {
        //             string content = await response.Content.ReadAsStringAsync();
        //             Console.WriteLine($"Échec de la suppression de l'employé {name} : {response.StatusCode}\n{content}");
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Exception lors de la suppression : {ex.Message}");
        //     }
        // }

        // public async Task UpdateEmpByName(string genre, DateOnly dateJ, string firstN, DateOnly dateB, string status, string middleN, string salutation, string lastN, string name)
        // {
        //     FrappeAuthHelper.AjouterAuthorization(_httpClient);

        //     string url = $"http://erpnext.localhost:8000/api/resource/Employee/{name}";

        //     var updatedData = new
        //     {
        //         gender = genre,
        //         date_of_joining = dateJ.ToString("yyyy-MM-dd"),
        //         first_name = firstN,
        //         date_of_birth = dateB.ToString("yyyy-MM-dd"),
        //         status = status,
        //         middle_name = middleN,
        //         salutation = salutation,
        //         last_name = lastN
        //     };

        //     // Sérialisation en JSON
        //     var content = new StringContent(JsonConvert.SerializeObject(updatedData), Encoding.UTF8, "application/json");
        //     // Envoi de la requête PUT
        //     var response = await _httpClient.PutAsync(url, content);

        //     if (!response.IsSuccessStatusCode)
        //     {
        //         var errorMsg = await response.Content.ReadAsStringAsync();
        //         throw new Exception($"Échec de la mise à jour : {response.StatusCode} - {errorMsg}");
        //     }
        // }

        // public async Task InsertEmployee(string genre, DateOnly dateJ, string firstN, DateOnly dateB, string status, string middleN, string salutation, string lastN)
        // {
        //     FrappeAuthHelper.AjouterAuthorization(_httpClient);

        //     string url = "http://erpnext.localhost:8000/api/resource/Employee";

        //     var newEmployee = new
        //     {
        //         gender = genre,
        //         date_of_joining = dateJ.ToString("yyyy-MM-dd"),
        //         first_name = firstN,
        //         date_of_birth = dateB.ToString("yyyy-MM-dd"),
        //         status = status,
        //         middle_name = middleN,
        //         salutation = salutation,
        //         last_name = lastN
        //     };

        //     var content = new StringContent(JsonConvert.SerializeObject(newEmployee), Encoding.UTF8, "application/json");
        //     var response = await _httpClient.PostAsync(url, content);

        //     if (!response.IsSuccessStatusCode)
        //     {
        //         var error = await response.Content.ReadAsStringAsync();
        //         throw new Exception($"Erreur lors de l'insertion de l'employé : {response.StatusCode} - {error}");
        //     }
        // }

    }
}
