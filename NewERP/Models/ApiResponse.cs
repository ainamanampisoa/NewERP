using System;
using Newtonsoft.Json;
namespace NewERP.Models
{
    public class ApiResponse<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}