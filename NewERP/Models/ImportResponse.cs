using Newtonsoft.Json;

namespace NewERP.Models
{
    public class ImportResponse<T>
    {
        [JsonProperty("message")]
        public T Data { get; set; }
    }
}
