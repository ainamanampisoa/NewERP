using NewERP.Models;
using NewERP.Helpers;
using Newtonsoft.Json;
using System.Globalization;

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
            var refsDejaVus = new HashSet<string>();
            var abbrDejaVus = new HashSet<string>();
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
                    }

                    string refId = colonnes[0].Trim();

                    if (refsDejaVus.Contains(refId))
                    {
                        erreurs.Add($"Erreur dans file1, ligne {i + 1} : Ref en double ({refId})");
                    }
                    else
                    {
                        refsDejaVus.Add(refId);
                    }

                    string dateEmbauche = colonnes[4].Trim();
                    string dateNaissance = colonnes[5].Trim();

                    if (!await ValideDate(dateEmbauche))
                    {
                        erreurs.Add($"Erreur dans file1, ligne {i + 1} : Date embauche invalide ({dateEmbauche})");
                    }

                    if (!await ValideDate(dateNaissance))
                    {
                        erreurs.Add($"Erreur dans file1, ligne {i + 1} : Date naissance invalide ({dateNaissance})");
                    }
                }


                // var lignesFichier2 = File.ReadAllLines(file2);
                // for (int i = 1; i < lignesFichier2.Length; i++)
                // {
                //     var colonnes = lignesFichier2[i].Split(',');

                //     if (colonnes.Length < 5)
                //     {
                //         erreurs.Add($"Erreur dans file2, ligne {i + 1} : Données insuffisantes.");
                //     }

                //     string refId = colonnes[2].Trim();

                //     if (abbrDejaVus.Contains(refId))
                //     {
                //         erreurs.Add($"Erreur dans file2, ligne {i + 1} : Abbreviation deja vu ({refId})");
                //     }
                //     else
                //     {
                //         abbrDejaVus.Add(refId);
                //     }
                    
                // }

                var lignesFichier3 = File.ReadAllLines(file3);
                for (int i = 1; i < lignesFichier3.Length; i++)
                {
                    var colonnes = lignesFichier3[i].Split(',');

                    if (colonnes.Length < 3)
                    {
                        erreurs.Add($"Erreur dans file3, ligne {i + 1} : Données insuffisantes.");
                        continue;
                    }

                    string mois = colonnes[0].Trim();

                    if (!await ValideDate(mois))
                    {
                        erreurs.Add($"Erreur dans file3, ligne {i + 1} : Date invalide ({mois})");
                    }

                    string salaire = colonnes[2].Trim();
                    if (!float.TryParse(salaire, NumberStyles.Float, CultureInfo.InvariantCulture, out float montant))
                    {
                        erreurs.Add($"Erreur dans file3, ligne {i + 1} : Nombre invalide ({salaire})");
                    }
                }
            }
            catch (Exception ex)
            {
                erreurs.Add("Erreur lors de l'import : " + ex.Message);
            }

            return erreurs;
        }

        public async Task<ImportResult> ImportDataCsv(string file1, string file2, string file3)
        {
            var result = new List<string>();

            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string url = "http://erpnext.localhost:8000/api/method/hrms.hr.import.importCsv";

            using (var content = new MultipartFormDataContent())
            {
                if (!string.IsNullOrEmpty(file1))
                {
                    var fileStream1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
                    content.Add(new StreamContent(fileStream1), "file1", Path.GetFileName(file1));
                }

                if (!string.IsNullOrEmpty(file2))
                {
                    var fileStream2 = new FileStream(file2, FileMode.Open, FileAccess.Read);
                    content.Add(new StreamContent(fileStream2), "file2", Path.GetFileName(file2));
                }

                if (!string.IsNullOrEmpty(file3))
                {
                    var fileStream3 = new FileStream(file3, FileMode.Open, FileAccess.Read);
                    content.Add(new StreamContent(fileStream3), "file3", Path.GetFileName(file3));
                }

                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Json : " + responseString);

                var wrappedResult = JsonConvert.DeserializeObject<ImportResponse<ImportResult>>(responseString);
                return wrappedResult.Data;
            }
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
