using System.Collections.Generic;
using Newtonsoft.Json;

namespace NewERP.Models
{
    public class ApiListResponse<T>
    {
        [JsonProperty("data")]
        public List<T> Data { get; set; }
    }
}
