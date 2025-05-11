using System;

namespace WebApp.Models
{
    public class AddMemberViewModel
    {
        public IFormFile? MemberImage { get; set; }
        public string MemberFirstName { get; set; } = string.Empty;
        public string MemberLastName { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
        public string MemberPhone { get; set; } = string.Empty;
        public string MemberJobTitle { get; set; } = string.Empty;
        public string MemberAddress { get; set; } = string.Empty;
        public DateOnly? MemberBirthDate { get; set; }
    }
}
