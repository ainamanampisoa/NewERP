using Newtonsoft.Json;

namespace NewERP.Models
{
    public class SalaryAssignment
    {
        [JsonProperty("name")]
        public string? name { get; set; }

        [JsonProperty("employee")]
        public string? Employee { get; set; }

        [JsonProperty("salary_structure")]
        public string? SalaryStructure { get; set; }

        [JsonProperty("base")]
        public float Base { get; set; }

        [JsonProperty("currency")]
        public string? Currency { get; set; }

        [JsonProperty("company")]
        public string? Company { get; set; }

        [JsonProperty("from_date")]
        public DateOnly? FromDate { get; set; }

        [JsonProperty("docstatus")]
        public int DocStatus { get; set; }

    }
    
}
