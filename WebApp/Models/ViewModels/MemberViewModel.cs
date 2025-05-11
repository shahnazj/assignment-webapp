using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Models
{
    public class MemberViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string MemberImage { get; set; } = null!;

        public string MemberFirstName { get; set; } = null!;
        public string MemberLastName { get; set; } = null!;
        public string MemberEmail { get; set; } = null!;
        public string MemberPhone { get; set; } = null!;
        public string MemberJobTitle { get; set; } = null!;
        public string MemberAddress { get; set; } = null!;
        public string MemberBirthDate { get; set; } = string.Empty;

    }
}