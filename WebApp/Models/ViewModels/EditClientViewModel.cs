using Microsoft.AspNetCore.Http;

namespace WebApp.Models
{
    public class EditClientViewModel
    {
        public Guid Id { get; set; }
        public IFormFile? ClientImage { get; set; }
        public string ClientName { get; set; } = string.Empty;
    }
}