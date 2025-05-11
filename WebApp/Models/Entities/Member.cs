using System;

namespace WebApp.Entities
{
    public class Member
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string MemberImage { get; set; } = null!;
        public string MemberFirstName { get; set; } = null!;
        public string MemberLastName { get; set; } = null!;
        public string MemberEmail { get; set; } = null!;
        public string MemberPhone { get; set; } = null!;
        public string MemberJobTitle { get; set; } = null!;
        public string MemberAddress { get; set; } = null!;
        public DateOnly MemberBirthDate { get; set; }
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
