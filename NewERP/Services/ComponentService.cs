using Newtonsoft.Json;
using NewERP.Models;
using NewERP.Helpers;

namespace NewERP.Services
{
    public class ComponentService
    {
        private readonly HttpClient _httpClient;

        public ComponentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SalaryComponent>> GetAllSalaryComponents()
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string url = "http://erpnext.localhost:8000/api/resource/Salary Component";
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            Console.WriteLine($"Status Code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur lors de la récupération des Salary Components: {response.StatusCode}");
                string errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Contenu de l'erreur: {errorContent}");
                return new List<SalaryComponent>();
            }

            string json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"JSON reçu: {json}");

            var responseData = JsonConvert.DeserializeObject<ApiResponse<List<SalaryComponent>>>(json);

            Console.WriteLine($"Nombre de composants désérialisés: {responseData?.Data?.Count ?? 0}");

            return responseData?.Data ?? new List<SalaryComponent>();
        }
        
        public async Task<bool> CreateSalaryComponent(string name, string abbr, string type)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            var data = new {
                doctype = "Salary Component",
                salary_component = name,             
                salary_component_abbr = abbr,      
                type = type,                   
                company = "IT Company",
                default_account = "Cash - IC"
            };

            string json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            string url = "http://erpnext.localhost:8000/api/resource/Salary Component";
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            Console.WriteLine($"Statut de création: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erreur lors de la création du Salary Component: {error}");
                return false;
            }

            return true;
        }


    }
}