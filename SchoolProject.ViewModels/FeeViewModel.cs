using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{

    public class FeeViewModel
    {
        private Fee fee;

        public FeeViewModel(Fee fee)
        {
            this.fee = fee;
        }

        public int Id { get; set; }

        public int StudentId { get; set; }

        public string? StudentName { get; set; }

        public int SubjectId { get; set; }

        public string? SubjectName { get; set; }

        public int SessionId { get; set; }

        public int Credits { get; set; }

        public decimal PricePerCredit { get; set; }

        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
