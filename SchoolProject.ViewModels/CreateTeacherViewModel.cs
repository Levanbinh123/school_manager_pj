using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolProject.Models;

namespace SchoolProject.ViewModels
{
    public class CreateTeacherViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }


        public DateTime DOB { get; set; }

        public DateTime DateOfJoin { get; set; } = DateTime.Now;

        public bool Selected { get; set; }

        public string? KeyId { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
        public int Experience { get; set; }

        public string Email {  get; set; }
        public string Qualification { get; set; }
        public Teacher ConvertModel(CreateTeacherViewModel teacher)
        {
            return new Teacher
            {
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                DOB = teacher.DOB,
                DateOfJoin = teacher.DateOfJoin,
                Selected = teacher.Selected,
                KeyId = teacher.KeyId,
                YearOfEx = teacher.Experience,
                Qualification = teacher.Qualification,
                Email = teacher.Email,
            };
        }
    }
}
