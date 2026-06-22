using Forge.Domain;

namespace Forge.Api.DTOs.Suppliers
{
    public class SupplierResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Currency { get; set; } = "PHP";
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }

        public static SupplierResponse FromEntity(Supplier supplier) => new()
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Currency = supplier.Currency,
            ContactPerson = supplier.ContactPerson,
            ContactEmail = supplier.ContactEmail,
            ContactPhone = supplier.ContactPhone,
            IsActive = supplier.IsActive
        };
    }
}