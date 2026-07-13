namespace Forge.Domain.Enums;

public static class StockMovementTypeExtensions
{
    public static bool IsIncrease(this StockMovementType type) => type switch
    {
        StockMovementType.ReceiptFromSupplier => true,
        StockMovementType.ReceiptFromProduction => true,
        StockMovementType.ReceiptFromSubcon => true,
        StockMovementType.AdjustmentIncrease => true,
        _ => false
    };

    public static bool IsDecrease(this StockMovementType type) => type switch
    {
        StockMovementType.IssuanceToProduction => true,
        StockMovementType.IssuanceToCustomer => true,
        StockMovementType.IssuanceToSubcon => true,
        StockMovementType.AdjustmentDecrease => true,
        _ => false
    };
}