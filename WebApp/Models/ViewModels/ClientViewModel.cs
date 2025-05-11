using System;

namespace WebApp.Models
{
    public class ClientViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ClientImage { get; set; } = null!;
        public string ClientName { get; set; } = null!;
    }
}
