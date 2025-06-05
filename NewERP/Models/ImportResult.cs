using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace NewERP.Models
{
    public class ImportResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }  

        [JsonProperty("type")]
        public string Type { get; set; }  

        [JsonProperty("advice")] 
        public string Advice { get; set; }

        [JsonProperty("details")]
        public ImportDetails Details { get; set; }
    }

    public class ImportDetails
    {
        [JsonProperty("employees_imported")]
        public int EmployeesImported { get; set; }

        [JsonProperty("files_processed")]
        public FileStatus FilesProcessed { get; set; }
    }

    public class FileStatus
    {
        [JsonProperty("employees")]
        public bool Employees { get; set; }

        [JsonProperty("salary_components")]
        public bool SalaryComponents { get; set; }

        [JsonProperty("salary_assignments")]
        public bool SalaryAssignments { get; set; }
    }


}