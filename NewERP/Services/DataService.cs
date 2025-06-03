using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        public async Task<List<string>> ImportData(string file1, string file2, string file3)
        {
            var erreurs = new List<string>();

            try
            {          
                FrappeAuthHelper.AjouterAuthorization(_httpClient);

                var lignesFichier1 = File.ReadAllLines(file1);
                for (int i = 1; i < lignesFichier1.Length; i++)
                {
                    var colonnes = lignesFichier1[i].Split(',');

                    if (colonnes.Length < 6)
                    {
                        erreurs.Add($"Erreur dans file1, ligne {i + 1} : Données insuffisantes.");
                        continue;
                    }

                    string dateEmbauche = colonnes[4].Trim();
                    string dateNaissance = colonnes[5].Trim();

                    // Formatage des dates valides
                    if (await ValideDate(dateEmbauche))
                    {
                        // DateTime embaucheDate = DateTime.ParseExact(dateEmbauche, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        // colonnes[0] = embaucheDate.ToString("yyyy-MM-dd"); // Format SQL
                    }
                    else
                    {
                        erreurs.Add($"Erreur dans file1, ligne {i + 1} : Date embauche invalide ({dateEmbauche})");
                    }

                    if (await ValideDate(dateNaissance))
                    {
                        // DateTime embaucheDate = DateTime.ParseExact(dateNaissance, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        // colonnes[0] = embaucheDate.ToString("yyyy-MM-dd"); // Format SQL
                    }
                    else
                    {
                        erreurs.Add($"Erreur dans file1, ligne {i + 1} : Date naissance invalide ({dateNaissance})");
                    }
                }

                var lignesFichier3 = File.ReadAllLines(file3);
                for (int i = 1; i < lignesFichier3.Length; i++)
                {
                    var colonnes = lignesFichier3[i].Split(',');

                    if (colonnes.Length < 1)
                    {
                        erreurs.Add($"Erreur dans file3, ligne {i + 1} : Données insuffisantes.");
                        continue;
                    }

                    string mois = colonnes[0].Trim();

                    if (await ValideDate(mois))
                    {
                        // DateTime embaucheDate = DateTime.ParseExact(mois, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        // colonnes[0] = embaucheDate.ToString("yyyy-MM-dd"); // Format SQL
                    }
                    else
                    {
                        erreurs.Add($"Erreur dans file3, ligne {i + 1} : Date invalide ({mois})");
                    }
                }
            }
            catch (Exception ex)
            {
                erreurs.Add("Erreur lors de l'import : " + ex.Message);
            }

            return erreurs;
        }

        public async Task<List<string>> ImportDataCsv(string file1, string file2, string file3)
        {
            var result = new List<string>();
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            // URL de l’endpoint Frappe
            string url = "http://erpnext.localhost:8000/api/method/hrms.hr.import.importCsv";

            // 1. Préparer le contenu multipart (les 3 fichiers)
            using (var content = new MultipartFormDataContent())
            {
                if (!string.IsNullOrEmpty(file1))
                    content.Add(new StreamContent(File.OpenRead(file1)), "file1", Path.GetFileName(file1));

                if (!string.IsNullOrEmpty(file2))
                    content.Add(new StreamContent(File.OpenRead(file2)), "file2", Path.GetFileName(file2));

                if (!string.IsNullOrEmpty(file3))
                    content.Add(new StreamContent(File.OpenRead(file3)), "file3", Path.GetFileName(file3));

                // 2. Créer manuellement la requête en HTTP/1.0
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content,
                    Version = new Version(1, 0)  // ← force HTTP/1.0, donc pas d’Expect: 100-continue
                };

                // 3. S’assurer qu’on n’a pas d’en-tête Expect sur la requête ou sur le client
                request.Headers.ExpectContinue = false;
                if (_httpClient.DefaultRequestHeaders.Contains("Expect"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Expect");
                }

                // 4. Envoyer la requête
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    result.Add("Succès : " + responseString);
                }
                else
                {
                    result.Add($"Erreur : {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }

            return result;
        }

        public async Task<bool> ValideDate(string date)
        {
            return await Task.FromResult(DateTime.TryParseExact(
                date,
                "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out _));
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