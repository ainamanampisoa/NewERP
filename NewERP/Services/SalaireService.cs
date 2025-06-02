using Newtonsoft.Json.Linq;
using NewERP.Models;
using NewERP.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Diagnostics;
using System.IO;
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

        public async Task<SalarySlip> GetSalarySlipParNomAsync(string slipName)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string url = $"http://erpnext.localhost:8000/api/resource/Salary Slip/{slipName}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseBody);

            var slip = json["data"].ToObject<SalarySlip>();
            return slip;
        }

        public void ExporterSalarySlipEnPdf(SalarySlip slip)
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Documents", "PDF");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"Bulletin_{slip.Employee}_{slip.StartDate:yyyyMM}.pdf";
            string outputPath = Path.Combine(folderPath, fileName);

            Document doc = new Document(PageSize.A4, 40f, 40f, 60f, 40f);
            PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));
            doc.Open();

            // Polices
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.DARK_GRAY);
            Font sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK);
            Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.DARK_GRAY);
            Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK);
            Font amountFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.DARK_GRAY);

            // Titre
            Chunk titleChunk = new Chunk("Fiche de paie", titleFont);
            titleChunk.SetUnderline(0.5f, -2f);
            Paragraph title = new Paragraph(titleChunk)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 25f
            };
            doc.Add(title);

            // Infos générales
            PdfPTable infoTable = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_LEFT };
            infoTable.SetWidths(new float[] { 1, 2 });
            infoTable.SpacingAfter = 20f;

            void AddInfoRow(string label, string value)
            {
                PdfPCell cellLabel = new PdfPCell(new Phrase(label, boldFont)) { Border = Rectangle.NO_BORDER };
                PdfPCell cellValue = new PdfPCell(new Phrase(value, normalFont)) { Border = Rectangle.NO_BORDER };
                infoTable.AddCell(cellLabel);
                infoTable.AddCell(cellValue);
            }

            AddInfoRow("Employé :", slip.EmployeeName);
            AddInfoRow("Matricule :", slip.Employee);
            AddInfoRow("Période :", $"{slip.StartDate:dd/MM/yyyy} - {slip.EndDate:dd/MM/yyyy}");
            AddInfoRow("Date paiement :", $"{slip.PostingDate:dd/MM/yyyy}");
            AddInfoRow("Statut :", slip.Status);

            doc.Add(infoTable);

            // Fonction pour afficher les tableaux
            void AddSalaryTable(string titleText, IEnumerable<SalaryComponentDetail> items, decimal total)
            {
                Paragraph sectionTitle = new Paragraph(titleText, sectionFont)
                {
                    SpacingBefore = 20f,
                    SpacingAfter = 10f
                };
                doc.Add(sectionTitle);

                PdfPTable table = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    SpacingAfter = 15f
                };
                table.SetWidths(new float[] { 3, 1 });

                // En-tête
                PdfPCell header1 = new PdfPCell(new Phrase("Composant", boldFont))
                {
                    BackgroundColor = new BaseColor(235, 235, 235),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6,
                    BorderColor = new BaseColor(180, 180, 180)
                };
                PdfPCell header2 = new PdfPCell(new Phrase("Montant", boldFont))
                {
                    BackgroundColor = new BaseColor(235, 235, 235),
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 6,
                    BorderColor = new BaseColor(180, 180, 180)
                };
                table.AddCell(header1);
                table.AddCell(header2);

                // Corps (lignes blanches avec bordures grises)
                foreach (var item in items)
                {
                    PdfPCell cellName = new PdfPCell(new Phrase(item.SalaryComponent, normalFont))
                    {
                        BackgroundColor = BaseColor.WHITE,
                        Padding = 5,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        BorderColor = new BaseColor(200, 200, 200)
                    };
                    PdfPCell cellAmount = new PdfPCell(new Phrase($"{item.Amount:N0} {slip.Currency}", amountFont))
                    {
                        BackgroundColor = BaseColor.WHITE,
                        Padding = 5,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        BorderColor = new BaseColor(200, 200, 200)
                    };
                    table.AddCell(cellName);
                    table.AddCell(cellAmount);
                }

                // Ligne Total
                PdfPCell totalLabel = new PdfPCell(new Phrase($"Total {titleText}", boldFont))
                {
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Padding = 7,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    BorderWidthTop = 1f,
                    BorderColor = new BaseColor(180, 180, 180)
                };
                PdfPCell totalValue = new PdfPCell(new Phrase($"{total:N0} {slip.Currency}", boldFont))
                {
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Padding = 7,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    BorderWidthTop = 1f,
                    BorderColor = new BaseColor(180, 180, 180)
                };
                table.AddCell(totalLabel);
                table.AddCell(totalValue);

                doc.Add(table);
            }

            // Sections : Gains et Déductions
            AddSalaryTable("Gains", slip.Earnings, slip.GrossPay);
            AddSalaryTable("Déductions", slip.Deductions, slip.TotalDeduction);

            // Net à payer
            Font netFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(0, 102, 204));
            Paragraph netPay = new Paragraph($"NET À PAYER : {slip.NetPay:N0} {slip.Currency}", netFont)
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingBefore = 30f
            };
            doc.Add(netPay);

            doc.Close();

            // Linux : ouvrir le fichier
            Process.Start("xdg-open", outputPath);
        }

        public async Task<List<SalarySlip>> GetSalarySlipsParMoisEtAnnee(int mois, int annee)
        {
            FrappeAuthHelper.AjouterAuthorization(_httpClient);

            string fields = "[\"name\", \"employee\", \"employee_name\", \"gross_pay\", \"total_deduction\", \"net_pay\", \"start_date\",\"status\"]";

            string dateDebut = new DateTime(annee, mois, 1).ToString("yyyy-MM-dd");
            string dateFin = new DateTime(annee, mois, DateTime.DaysInMonth(annee, mois)).ToString("yyyy-MM-dd");

            // Ici on filtre par start_date et pas posting_date
            string filters = $@"[
                [""start_date"", "">="", ""{dateDebut}""],
                [""start_date"", ""<="", ""{dateFin}""]
            ]";

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