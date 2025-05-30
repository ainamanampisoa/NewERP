using Newtonsoft.Json;

namespace NewERP.Models
{
    public class Employe
    {
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("employee_name")]
        public string employee_name { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("department")]
        public string department { get; set; }
    }
}
