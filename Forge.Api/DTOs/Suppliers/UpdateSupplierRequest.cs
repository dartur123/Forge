using System.ComponentModel.DataAnnotations;

namespace Forge.Api.DTOs.Suppliers;

public class UpdateSupplierRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(3)]
    [MaxLength(3)]
    public string Currency { get; set; } = "PHP";

    public string? ContactPerson { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }
}

