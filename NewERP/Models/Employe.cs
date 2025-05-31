using System;
using Newtonsoft.Json;

namespace NewERP.Models
{
    public class Employe
    {
        [JsonProperty("name")]
        public string name { get; set; }  // ID ERPNext (ex: HR-EMP-00001)

        [JsonProperty("employee_name")]
        public string employee_name { get; set; }

        [JsonProperty("employee_number")]
        public string employee_number { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }

        [JsonProperty("gender")]
        public string gender { get; set; }

        [JsonProperty("date_of_birth")]
        public DateTime? date_of_birth { get; set; }

        [JsonProperty("date_of_joining")]
        public DateTime? date_of_joining { get; set; }

        [JsonProperty("department")]
        public string department { get; set; }

        [JsonProperty("designation")]
        public string designation { get; set; }

        [JsonProperty("reports_to")]
        public string reports_to { get; set; }

        [JsonProperty("prefered_email")]
        public string prefered_email { get; set; }

        [JsonProperty("marital_status")]
        public string marital_status { get; set; }

        [JsonProperty("cell_number")]
        public string cell_number { get; set; }

        [JsonProperty("current_address")]
        public string current_address { get; set; }
    }
}
