using Newtonsoft.Json.Linq;
using NewERP.Models;
using NewERP.Helpers;

namespace NewERP.Services
{
    public class SalaireService
    {
        private readonly HttpClient _httpClient;

        public SalaireService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SalarySlip>> GetSalarySlipsParEmployeId(string employeeId)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string fields = "[\"name\", \"employee\", \"employee_name\", \"start_date\", \"end_date\", \"gross_pay\", \"total_deduction\", \"net_pay\", \"status\"]";
            string filters = $"[[\"employee\", \"=\", \"{employeeId}\"]]";

            string url = $"http://erpnext.localhost:8000/api/resource/Salary Slip?fields={Uri.EscapeDataString(fields)}&filters={Uri.EscapeDataString(filters)}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var salarySlips = json["data"].ToObject<List<SalarySlip>>();
            return salarySlips;
        }


    }
}