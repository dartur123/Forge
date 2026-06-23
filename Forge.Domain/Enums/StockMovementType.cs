namespace Forge.Domain.Enums;

public enum StockMovementType
{
    ReceiptFromSupplier,      // raw materials coming in from supplier
    ReceiptFromProduction,    // finished goods coming in from production floor
    ReceiptFromSubcon,        // materials returned/output from subcontractor
    IssuanceToProduction,     // raw materials going out to production floor
    IssuanceToCustomer,       // finished goods going out to customer
    IssuanceToSubcon,         // materials going out to subcontractor
    Transfer,                 // moving between locations (Warehouse A → Rack 1)
    Adjustment                // manual correction (variance, damage, count discrepancy)
}
