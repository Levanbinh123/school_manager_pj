
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
   public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public DateTime DateOfJoin { get; set; }
            
        public string KeyId { get; set; }
        public int MajorId { get; set; }

        public Major Major { get; set; }
        public int YearLevel { get; set; }
        public ICollection<Enroll> Enrolls { get; set; }=new HashSet<Enroll>();
    }
}
