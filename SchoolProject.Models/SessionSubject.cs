namespace SchoolProject.Models
{
    public class SessionSubject
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        public Session Session { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public bool IsOpen { get; set; } = true;
    }
}