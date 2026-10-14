using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class TuitionRate
    {
        public int Id { get; set; }

        // Năm học
        public int SessionId { get; set; }

        // Giá 1 tín chỉ trong năm học đó
        public decimal PricePerCredit { get; set; }

        public Session Session { get; set; } = null!;
    }
}
