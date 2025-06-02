using Newtonsoft.Json;

namespace NewERP.Models
{
    public class Gender
    {
        [JsonProperty("name")]
        public string name { get; set; } // généralement "Male", "Female", etc.

        [JsonProperty("gender")]
        public string gender { get; set; } // parfois le même que name
    }
}
