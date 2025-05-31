using System.Net.Http;

namespace NewERP.Helpers
{
    public static class FrappeAuthHelper
    {
        private const string apiKey = "83c15544524c27f";
        private const string apiSecret = "cccad3a5b5bf570";

        public static void AjouterAuthorization(HttpClient client)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"token {apiKey}:{apiSecret}");
        }
    }

    // public static class FrappeAuthHelper
    // {
    //     private static string _sid;

    //     public static async Task AjouterAuthorization(HttpClient client)
    //     {
    //         if (string.IsNullOrEmpty(_sid))
    //         {
    //             _sid = await ObtenirSidAsync(client);
    //         }

    //         // Ajouter le cookie à l'en-tête
    //         client.DefaultRequestHeaders.Clear();
    //         client.DefaultRequestHeaders.Add("Cookie", $"sid={_sid}");
    //     }

    //     private static async Task<string> ObtenirSidAsync(HttpClient client)
    //     {
    //         var loginData = new Dictionary<string, string>
    //         {
    //             { "usr", "Administrator" },
    //             { "pwd", "admin" }
    //         };

    //         var content = new FormUrlEncodedContent(loginData);
    //         var response = await client.PostAsync("http://erpnext.localhost:8000/api/method/login", content);
    //         response.EnsureSuccessStatusCode();

    //         // Extraire le cookie SID depuis les en-têtes Set-Cookie
    //         if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
    //         {
    //             var sidCookie = cookies
    //                 .Select(c => c.Split(';').FirstOrDefault(part => part.Trim().StartsWith("sid=")))
    //                 .FirstOrDefault(part => part != null);

    //             if (!string.IsNullOrEmpty(sidCookie))
    //             {
    //                 return sidCookie.Split('=')[1];
    //             }
    //         }

    //         throw new Exception("Impossible d'obtenir le cookie SID.");
    //     }
    // }
}