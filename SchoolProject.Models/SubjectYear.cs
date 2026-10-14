namespace SchoolProject.Models
{
    public class SubjectYear
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }

        public Subject Subject { get; set; }

        // 1 = năm 1
        // 2 = năm 2
        // 3 = năm 3
        // 4 = năm 4
        public int YearLevel { get; set; }
    }
}