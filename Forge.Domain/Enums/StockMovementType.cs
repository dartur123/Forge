namespace Forge.Domain.Enums;

public enum StockMovementType
{
    SupplierReceipt,
    ProductionIssuance,
    ComponentReceipt,
    ComponentIssuance,
    FinishedGoodReceipt,
    CustomerIssuance,
    SubconIssuance,
    SubconReceipt,
    Transfer,
    Adjustment
}