using Newtonsoft.Json;

namespace NewERP.Models
{
    public class SalaryStatView
    {
        public DateTime StartDate { get; set; }
        public decimal GrossPay { get; set; }
        public decimal NetPay { get; set; }
        public decimal TotalDeduction { get; set; }
    }
}
