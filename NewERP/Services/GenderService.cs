using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NewERP.Models;
using NewERP.Helpers;

namespace NewERP.Services
{
    public class GenderService
    {
        private readonly HttpClient _httpClient;

        public GenderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Gender>> GetAllGenders()
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string url = "http://erpnext.localhost:8000/api/resource/Gender?fields=[\"name\"]";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var data = json["data"].ToObject<List<Gender>>();
            return data;
        }
    }
}
