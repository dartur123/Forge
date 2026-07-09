using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class Subcontractor
{
    protected Subcontractor() { }
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Currency { get; private set; } = "PHP";
    public string? Specialization { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; } = true;
    public static Subcontractor Create(string name, string currency = "PHP", string? specialization = null, string? contactPerson = null, string? contactEmail = null, string? contactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty");

        return new Subcontractor
        {
            Name = name,
            Currency = currency,
            Specialization = specialization,
            ContactPerson = contactPerson,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone
        };
    }

    public void Deactivate() => IsActive = false;
    public void Reactivate() => IsActive = true;
}