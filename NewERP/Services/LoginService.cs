using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NewERP.Services
{
    public class LoginService
    {
        private readonly HttpClient _client;

        public LoginService(HttpClient client)
        {
            _client = client;
        }
        public async Task<string?> Login(string username, string password)
        {
            try
            {
                var loginData = new Dictionary<string, string>
                {
                    { "usr", username },
                    { "pwd", password }
                };

                var formContent = new FormUrlEncodedContent(loginData);
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };

                using var httpClient = new HttpClient(handler);
                httpClient.BaseAddress = new Uri("http://erpnext.localhost:8000/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.PostAsync("api/method/login", formContent);

                if (response.IsSuccessStatusCode)
                {
                    // Récupérer le cookie 'sid' (session id)
                    var cookies = handler.CookieContainer.GetCookies(new Uri("http://erpnext.localhost:8000/"));
                    var sidCookie = cookies["sid"];

                    if (sidCookie != null)
                    {
                        return sidCookie.Value;
                    }
                }
                else
                {
                    Console.WriteLine($"Échec de la connexion : {response.StatusCode}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors de la connexion : {ex.Message}");
                return null;
            }
        }

    }
}