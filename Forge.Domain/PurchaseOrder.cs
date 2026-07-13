using Forge.Domain.Enums;
using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class PurchaseOrder
{
    protected PurchaseOrder()
    {

    }
    public int Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public int SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;
    public string Currency { get; private set; } = "PHP";
    public decimal ExchangeRate { get; private set; } = 1.0m;
    public int CreatedByUserId { get; private set; }
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
    public bool IsSentToSupplier { get; private set; } = false;

    public List<PurchaseOrderLine> Lines { get; private set; } = new();

    public static PurchaseOrder Create(string orderNumber, int supplierId, string currency, decimal exchangeRate, int createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(orderNumber)) 
        {
            throw new DomainException("Order number is required.");
        }

        return new PurchaseOrder
        {
            OrderNumber = orderNumber,
            SupplierId = supplierId,
            Currency = currency,
            ExchangeRate = exchangeRate,
            CreatedByUserId = createdByUserId
        };
    }

    public void Submit() 
    {
        if (!Lines.Any())
        {
            throw new DomainException("Cannot submit a purchase order without any lines.");
        }

        if (Status == PurchaseOrderStatus.Draft)
            Status = PurchaseOrderStatus.Submitted;
        else
            throw new DomainException("Only draft purchase orders can be submitted.");
    }
    public void Approve()
    {
        if (Status == PurchaseOrderStatus.Submitted)
            Status = PurchaseOrderStatus.Approved;
        else
            throw new DomainException("Only submitted purchase orders can be approved.");
    }
    public void Reject()
    {
        if (Status == PurchaseOrderStatus.Submitted)
            Status = PurchaseOrderStatus.Rejected;
        else
            throw new DomainException("Only submitted purchase orders can be rejected.");
    }
    public void Return() 
    {
        if (Status == PurchaseOrderStatus.Submitted)
            Status = PurchaseOrderStatus.Returned;
        else
            throw new DomainException("Only submitted purchase orders can be returned.");
    }
    public void Cancel()
    {
        if(Status==PurchaseOrderStatus.Submitted || (Status==PurchaseOrderStatus.Approved && !IsSentToSupplier))
        {
            Status = PurchaseOrderStatus.Cancelled;
        }
        else
            throw new DomainException("Only submitted or approved (not sent to supplier) purchase orders can be cancelled.");
    }
    public void MarkAsSentToSupplier()
    {
        if (Status == PurchaseOrderStatus.Approved)
            IsSentToSupplier = true;
        else
            throw new DomainException("Only approved purchase orders can be marked as sent to supplier.");
    }

    public void StartEditing()
    {
        if (Status == PurchaseOrderStatus.Returned)
            Status = PurchaseOrderStatus.Draft;
        else
            throw new DomainException("Only returned purchase orders can be edited.");
    }

    private bool IsAllowedToAddEditLine()
    {
        return Status == PurchaseOrderStatus.Draft || Status == PurchaseOrderStatus.Submitted
            || Status == PurchaseOrderStatus.Returned || Status==PurchaseOrderStatus.Approved;
    }

    public void AddLine(PurchaseOrderLine line)
    {
        if(!IsAllowedToAddEditLine())
            throw new DomainException("Cannot add line to purchase order in its current status.");

        Lines.Add(line);
        Status = PurchaseOrderStatus.Draft; // Reset status to Draft when a line is added
    }

    public void EditLine(int lineId, PurchaseOrderLine updatedLine)
    {
        if (!IsAllowedToAddEditLine())
            throw new DomainException("Cannot edit line in purchase order in its current status.");
        var existingLine = Lines.FirstOrDefault(l => l.Id == lineId);
        if (existingLine == null)
            throw new DomainException("Line not found.");
        existingLine.Update(updatedLine.MaterialId, updatedLine.Quantity, updatedLine.UnitCostForeign);
        Status = PurchaseOrderStatus.Draft; // Reset status to Draft when a line is edited
    }

    public void RemoveLine(int lineId)
    {
        if (!IsAllowedToAddEditLine())
            throw new DomainException("Cannot remove line from purchase order in its current status.");
        var lineToRemove = Lines.FirstOrDefault(l => l.Id == lineId);
        if (lineToRemove == null)
            throw new DomainException("Line not found.");
        Lines.Remove(lineToRemove);
        Status = PurchaseOrderStatus.Draft; // Reset status to Draft when a line is removed
    }
}
