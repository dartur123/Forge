using Forge.Domain;

namespace Forge.Application.Responses
{
    public class SupplierResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }

        public static SupplierResult FromEntity(Supplier supplier)
        {
            return new SupplierResult
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
}
