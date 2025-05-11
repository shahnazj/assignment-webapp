using System.Collections.Generic;

namespace WebApp.Models
{
    public class ClientsViewModel
    {
        public IEnumerable<ClientViewModel> Clients { get; set; } = [];
        public AddClientViewModel AddClientFormData { get; set; } = new();
        public EditClientViewModel EditClientFormData { get; set; } = new();
    }
}