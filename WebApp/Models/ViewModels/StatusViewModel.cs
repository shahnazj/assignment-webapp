namespace WebApp.Models
{
    public class StatusViewModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string StatusName { get; set; } = null!;
    }
}
