//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SchoolProject.Models
//{
//    public class Grade
//    {
//        public int Id { get; set; }
//        public string Name { get; set; }
//        public ICollection<AssignGrade> AssignGrades { get; set; }=new HashSet<AssignGrade>();
//        public ICollection<Class> Classes { get; set; }
//       = new HashSet<Class>();

//        public ICollection<GradeSubject> GradeSubjects { get; set; } = new HashSet<GradeSubject>();
//    }
//}
