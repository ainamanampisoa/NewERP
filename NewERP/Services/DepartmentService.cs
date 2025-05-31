using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using NewERP.Models;
using NewERP.Helpers;

public class DepartmentService
{
    private readonly HttpClient _httpClient;

    public DepartmentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Department>> GetAllDepartments()
    {
        FrappeAuthHelper.AjouterAuthorization(_httpClient);

        string url = "http://erpnext.localhost:8000/api/resource/Department?fields=[\"name\"]";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(responseBody);

        var data = json["data"].ToObject<List<Department>>();
        return data;
    }
}
