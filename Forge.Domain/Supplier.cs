using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class Supplier
{
    protected Supplier() { }
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Currency { get; private set; } = "PHP";
    public string? ContactPerson { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; } = true;
    public List<Lot> Lots { get; private set; } = new();

    public static Supplier Create(string name, string currency, string? contactPerson, string? contactEmail, string? contactPhone)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required.");

        if(string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Supplier currency is required.");

        return new Supplier
        {
            Name = name,
            Currency = currency,
            ContactPerson = contactPerson,
            ContactEmail = contactEmail,
            ContactPhone = contactPhone
        };
    }

    public void Update(string name, string currency, string? contactPerson, string? contactEmail, string? contactPhone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Supplier currency is required.");
        Name = name;
        Currency = currency;
        ContactPerson = contactPerson;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
    }

    public void Deactivate() => IsActive = false;
}