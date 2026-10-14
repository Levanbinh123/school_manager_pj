using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Credits { get; set; }

        // =========================
        // THUỘC NGÀNH
        // =========================

        public int MajorId { get; set; }

        public Major Major { get; set; }
        // =========================
        // MÔN DÀNH CHO NĂM NÀO
        // =========================

        public ICollection<SubjectYear> SubjectYears { get; set; }
            = new HashSet<SubjectYear>();
    }
}
