// Models/CpiData.cs
namespace CpiWebService.Models
{
    public class CpiData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int CPI { get; set; }
        public string? Notes { get; set; }
    }
}
