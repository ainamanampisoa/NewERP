# NewERP
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Diagnostics;
using System.IO;
using NewERP.Models;

public class PdfService
{
    public void ExporterSalarySlipEnPdf(SalarySlip slip)
    {
        var document = new PdfDocument();
        document.Info.Title = $"Bulletin de salaire - {slip.EmployeeName}";
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        var font = new XFont("Verdana", 10, XFontStyle.Regular);
        double y = 40;

        void DrawLine(string label, string value)
        {
            gfx.DrawString($"{label} :", font, XBrushes.Black, new XRect(40, y, page.Width, 20), XStringFormats.TopLeft);
            gfx.DrawString(value, font, XBrushes.Black, new XRect(200, y, page.Width, 20), XStringFormats.TopLeft);
            y += 20;
        }

        // Informations principales
        DrawLine("Employé", slip.EmployeeName);
        DrawLine("Matricule", slip.Employee);
        DrawLine("Département", slip.Department);
        DrawLine("Poste", slip.Designation);
        DrawLine("Période", $"{slip.StartDate:dd/MM/yyyy} - {slip.EndDate:dd/MM/yyyy}");
        DrawLine("Date de paiement", slip.PostingDate.ToShortDateString());
        DrawLine("Statut", slip.Status);
        y += 20;

        // GAIN
        gfx.DrawString("== GAIN ==", font, XBrushes.DarkGreen, new XRect(40, y, page.Width, 20), XStringFormats.TopLeft);
        y += 20;
        foreach (var e in slip.Earnings)
        {
            DrawLine(e.SalaryComponent, $"{e.Amount:N0} {slip.Currency}");
        }

        DrawLine("Total Gain", $"{slip.TotalEarning:N0} {slip.Currency}");
        y += 20;

        // DÉDUCTION
        gfx.DrawString("== DÉDUCTIONS ==", font, XBrushes.DarkRed, new XRect(40, y, page.Width, 20), XStringFormats.TopLeft);
        y += 20;
        foreach (var d in slip.Deductions)
        {
            DrawLine(d.SalaryComponent, $"{d.Amount:N0} {slip.Currency}");
        }

        DrawLine("Total Déductions", $"{slip.TotalDeduction:N0} {slip.Currency}");
        y += 30;

        // NET À PAYER
        gfx.DrawString($"NET À PAYER : {slip.NetPay:N0} {slip.Currency}", new XFont("Verdana", 12, XFontStyle.Bold),
            XBrushes.Black, new XRect(40, y, page.Width, 20), XStringFormats.TopLeft);

        // Sauvegarde
        string fileName = $"Bulletin_{slip.Employee}_{slip.StartDate:yyyyMM}.pdf";
        string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        document.Save(outputPath);

        // Ouvrir automatiquement (optionnel)
        Process.Start("explorer.exe", outputPath);
    }
}
