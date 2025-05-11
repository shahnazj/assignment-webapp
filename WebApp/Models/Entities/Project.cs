using System;
using System.Collections.Generic;

namespace WebApp.Entities
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ProjectImage { get; set; } = null!;
        public string ProjectName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public DateTime? Deadline { get; set; }
        public Guid StatusId { get; set; }
        public Status Status { get; set; }
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public ICollection<Member> Members { get; set; } = new List<Member>();
    }
}
