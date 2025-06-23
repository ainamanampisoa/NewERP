using Newtonsoft.Json;

namespace NewERP.Models
{
    public class SalaryComponent
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("salary_component_abbr")]
        public string Abbr { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

    }

}