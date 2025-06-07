using Microsoft.EntityFrameworkCore;

namespace MCOP.Data.Models
{
    [PrimaryKey(nameof(Id))]
    public class ApiUsage
    {
        public int Id { get; set; }
        public string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
        public int Count { get; set; }
    }
}
