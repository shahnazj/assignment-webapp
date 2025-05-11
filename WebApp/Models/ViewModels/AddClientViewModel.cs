using Microsoft.AspNetCore.Http;

namespace WebApp.Models
{
    public class AddClientViewModel
    {
        public IFormFile? ClientImage { get; set; }
        public string ClientName { get; set; } = string.Empty;
    }
}