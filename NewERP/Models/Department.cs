using System;
using Newtonsoft.Json;
namespace NewERP.Models
{
    public class Department
    {
        [JsonProperty("name")]
        public string name { get; set; }  // C'est le nom du département
    }
}