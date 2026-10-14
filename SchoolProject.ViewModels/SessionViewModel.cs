using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class SessionViewModel
    {
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int MaxCredits { get; set; } = 30;
        public string Name { get; set; } = string.Empty;
        // public List<GradeViewModel> gradeViewModels { get; set; }
        public string Combined
        {
            get
            {
                return Start + "-" + End;
            }
        }
        public SessionViewModel() { }
        public SessionViewModel(Session model)
        {
            Id=model.Id;
            Start=model.Start;
            End=model.End;
            MaxCredits=model.MaxCredits;
            Name = model.Name;
        }
    }
}
