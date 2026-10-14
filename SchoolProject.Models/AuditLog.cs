using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Action { get; set; }

        public DateTime CreatedAt { get; set; }
        public ApplicationUser User {  get; set; }
    }
}
