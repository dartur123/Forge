using Forge.Domain.Enums;

namespace Forge.Domain;

public class CompanySettings
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public CostingMethod CostingMethod { get; set; } = CostingMethod.WeightedAverage;
    public string BaseCurrency { get; set; } = "PHP";
}

