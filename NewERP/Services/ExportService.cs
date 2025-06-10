using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NewERP.Services
{
    public class ExportService
    {
        private readonly HttpClient _httpClient;
        private readonly SalaireService _salaireService;

        public ExportService(HttpClient httpClient, SalaireService salaireService)
        {
            _httpClient = httpClient;
            _salaireService = salaireService;
        }

        public static void ExportToCsv<T>(List<T> data, string filePath)
        {
            var csv = new StringBuilder();
            var type = typeof(T);

            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsPrimitive || 
                           p.PropertyType == typeof(string) || 
                           p.PropertyType == typeof(DateTime) || 
                           p.PropertyType == typeof(DateTime?) || 
                           p.PropertyType == typeof(decimal) || 
                           p.PropertyType == typeof(decimal?))
                .ToList();

            // Ajouter l'en-tête
            csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Ajouter les données
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item);
                    return Escape(value?.ToString() ?? "");
                });

                csv.AppendLine(string.Join(",", values));
            }

            // S'assurer que le répertoire existe
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";
                
            // Échapper les guillemets doubles et entourer de guillemets
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}