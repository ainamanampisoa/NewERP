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

        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        [JsonProperty("end_date")]
        public string EndDate { get; set; }

        [JsonProperty("gross_pay")]
        public decimal GrossPay { get; set; }

        [JsonProperty("total_deduction")]
        public decimal TotalDeduction { get; set; }

        [JsonProperty("net_pay")]
        public decimal NetPay { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
