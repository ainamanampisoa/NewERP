using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NewERP.Helpers;

namespace NewERP.Services
{
    public class DataService
    {
        private readonly HttpClient _httpClient;

        public DataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ImportCsvFilesAsync(string employeesCsv, string structureCsv, string slipsCsv)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            var parameters = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrWhiteSpace(employeesCsv))
                parameters.Add(new KeyValuePair<string, string>("employees_csv", employeesCsv));
            if (!string.IsNullOrWhiteSpace(structureCsv))
                parameters.Add(new KeyValuePair<string, string>("structure_csv", structureCsv));
            if (!string.IsNullOrWhiteSpace(slipsCsv))
                parameters.Add(new KeyValuePair<string, string>("slips_csv", slipsCsv));

            var content = new FormUrlEncodedContent(parameters);

            var response = await _httpClient.PostAsync(
                "http://erpnext.localhost:8000/api/method/hrms.hr.import.import_all_data",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erreur API: {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> ResetDataAsync()
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            var response = await _httpClient.PostAsync(
                "http://erpnext.localhost:8000/api/method/hrms.hr.reset.reset_data",
                null // Aucune donnée envoyée, donc null
            );

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erreur API: {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }

    }
}
