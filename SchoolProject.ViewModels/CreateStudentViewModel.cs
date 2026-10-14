using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class CreateStudentViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }   
        public DateTime DateOfJoin { get; set; }=DateTime.Now;
        [Range(1, 4, ErrorMessage = "Năm học phải từ 1 đến 4.")]
        public int YearLevel { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "MajorId không hợp lệ.")]
        public int MajorId { get; set; }
        public string? KeyId {  get; set; }
        public string UserName { get; set; }
        public string Email {  get; set; }
        public string? Password { get; set; }
        public Student ConvertModel(CreateStudentViewModel student)
        {
            return new Student
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                DOB = student.DOB,
                DateOfJoin = student.DateOfJoin,
                KeyId = student.KeyId,
                MajorId = student.MajorId,
                YearLevel = student.YearLevel
            };
        }
    }
}
