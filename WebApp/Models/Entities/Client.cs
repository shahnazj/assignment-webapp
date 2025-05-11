using System;
using System.Collections.Generic;

namespace WebApp.Entities
{
    public class Client
    {
        public Guid Id { get; set; }
        public string ClientImage { get; set; } = null!;
        public string ClientName { get; set; } = null!;
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
