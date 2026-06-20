namespace Forge.Domain;

public class Subcontractor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "PHP";
    public string? Specialization { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}