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
        // ===============
        // =========================
        // MÔN DÀNH CHO NĂM NÀO
        // =========================
        // Một môn có thể được nhiều ngành sử dụng
        public ICollection<MajorSubject> MajorSubjects { get; set; }
            = new HashSet<MajorSubject>();
      
    }
}
