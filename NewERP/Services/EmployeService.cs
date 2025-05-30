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
            await FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string url = "http://erpnext.localhost:8000/api/resource/Employee?fields=[\"name\",\"employee_name\",\"status\",\"department\"]";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var data = json["data"].ToObject<List<Employe>>();

            return data;
        }
    }
}
