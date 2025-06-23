using Newtonsoft.Json;

namespace NewERP.Models
{
    public class SalaryStructure
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("is_active")]
        public int IsActive { get; set; }
    }
}