using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class TeacherViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DOB { get; set; }

        public DateTime DateOfJoin { get; set; }

        public bool Selected { get; set; }

        public string Qualification { get; set; }

        public int YearOfEx { get; set; }

        public TeacherViewModel()
        {
        }
        public TeacherViewModel(Teacher model)
        {
            Id = model.Id;
            FirstName = model.FirstName;
            LastName = model.LastName;
            DOB = model.DOB;
            DateOfJoin = model.DateOfJoin;
            Selected = model.Selected;

            Qualification = model.Qualification;
            YearOfEx = model.YearOfEx;
        }
    }
}
