using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace WebApp.Models
{
    public class EditProjectViewModel
    {
        public Guid Id { get; set; }
        public IFormFile ProjectImage { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);
        public Guid MemberId { get; set; }
        public decimal Budget { get; set; }
        public Guid StatusId { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; } = [];
        public IEnumerable<SelectListItem> Members { get; set; } = [];
        public IEnumerable<SelectListItem> Statuses { get; set; } = [];
    }
}