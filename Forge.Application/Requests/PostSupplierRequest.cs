namespace Forge.Application.Requests
{
    public class PostSupplierRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Currency { get; set; } = "PHP";
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }
}
