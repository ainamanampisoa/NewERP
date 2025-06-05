using Newtonsoft.Json;

namespace NewERP.Models
{
    public class SalarySlip
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("employee")]
        public string Employee { get; set; }

        [JsonProperty("employee_name")]
        public string EmployeeName { get; set; }

        [JsonProperty("posting_date")]
        public DateTime PostingDate { get; set; }

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; }

        [JsonProperty("gross_pay")]
        public decimal GrossPay { get; set; }

        [JsonProperty("net_pay")]
        public decimal NetPay { get; set; }

        [JsonProperty("total_deduction")]
        public decimal TotalDeduction { get; set; }

        [JsonProperty("total_earning")]
        public decimal TotalEarning { get; set; }

        [JsonProperty("base")]
        public decimal Base { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("designation")]
        public string Designation { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("branch")]
        public string Branch { get; set; }

        [JsonProperty("payroll_entry")]
        public string PayrollEntry { get; set; }

        [JsonProperty("salary_structure")]
        public string SalaryStructure { get; set; }

        [JsonProperty("leave_without_pay")]
        public decimal LeaveWithoutPay { get; set; }

        [JsonProperty("payment_days")]
        public decimal PaymentDays { get; set; }

        [JsonProperty("arrear_amount")]
        public decimal ArrearAmount { get; set; }

        [JsonProperty("incentive_amount")]
        public decimal IncentiveAmount { get; set; }

        [JsonProperty("remarks")]
        public string Remarks { get; set; }

        [JsonProperty("earnings")]
        public List<SalaryComponentDetail> Earnings { get; set; }

        [JsonProperty("deductions")]
        public List<SalaryComponentDetail> Deductions { get; set; }
    }

    public class SalaryComponentDetail
    {
        [JsonProperty("salary_component")]
        public string SalaryComponent { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("doctype")]
        public string DocType { get; set; }

        [JsonProperty("idx")]
        public int Idx { get; set; }
    }

    // Classe pour représenter les totaux mensuels
    public class MonthlySalaryTotals
    {
        public int Mois { get; set; }
        public string NomMois { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalNetPay { get; set; }
        public decimal TotalDeduction { get; set; }
        public Dictionary<string, decimal> TotalEarningsByComponent { get; set; }
        public Dictionary<string, decimal> TotalDeductionsByComponent { get; set; }
        public int NombreBulletins { get; set; }
    }

    // Classe pour les totaux annuels
    public class AnnualSalaryTotals
    {
        public int Annee { get; set; }
        public List<MonthlySalaryTotals> TotauxMensuels { get; set; }
        public decimal TotalAnnuelGrossPay { get; set; }
        public decimal TotalAnnuelNetPay { get; set; }
        public decimal TotalAnnuelDeduction { get; set; }
    }
}