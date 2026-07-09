using Forge.Domain.Enums;
using Forge.Domain.Exceptions;

namespace Forge.Domain;

public class SubconOrder
{
    protected SubconOrder() { }
    public int Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public int SubcontractorId { get; private set; }
    public Subcontractor Subcontractor { get; private set; } = null!;
    public SubconOrderStatus Status { get; private set; } = SubconOrderStatus.Draft;
    public string Currency { get; private set; } = "PHP";
    public decimal ExchangeRate { get; private set; } = 1.0m;
    public int CreatedByUserId { get; private set; }
    public User CreatedByUser { get; private set; } = null!;
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
    public bool IsSentToSupplier { get; private set; } = false;

    public List<SubconOrderLine> Lines { get; private set; } = new();

    public static SubconOrder Create(string orderNumber, int subcontractorId, string currency, decimal exchangeRate, int createdByUserId)
    {
        if(string.IsNullOrWhiteSpace(orderNumber))
            throw new DomainException("Order number cannot be empty.");

        if(subcontractorId <= 0)
            throw new DomainException("Subcontractor is required.");

        if(string.IsNullOrEmpty(currency))
            throw new DomainException("Currency is required.");

        if(exchangeRate <= 0)
            throw new DomainException("Exchange rate must be greater than zero.");

        if(createdByUserId <= 0)
            throw new DomainException("Created by user is required.");

        return new SubconOrder
        {
            OrderNumber = orderNumber,
            SubcontractorId = subcontractorId,
            Currency = currency,
            ExchangeRate = exchangeRate,
            CreatedByUserId = createdByUserId,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void Update(string orderNumber, int subcontractorId, string currency, decimal exchangeRate)
    {
        if(!IsAllowedToAddEditLine())
            throw new DomainException("Cannot update subcon order in its current status.");
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new DomainException("Order number cannot be empty.");
        if (subcontractorId <= 0)
            throw new DomainException("Subcontractor is required.");
        if (string.IsNullOrEmpty(currency))
            throw new DomainException("Currency is required.");
        if (exchangeRate <= 0)
            throw new DomainException("Exchange rate must be greater than zero.");
        OrderNumber = orderNumber;
        SubcontractorId = subcontractorId;
        Currency = currency;
        ExchangeRate = exchangeRate;
    }

    public void Submit()
    {
        if (!Lines.Any())
        {
            throw new DomainException("Cannot submit a subcon order without any lines.");
        }

        if (Status == SubconOrderStatus.Draft)
            Status = SubconOrderStatus.Submitted;
        else
            throw new DomainException("Only draft subcon orders can be submitted.");
    }
    public void Approve()
    { 
        if (Status == SubconOrderStatus.Submitted)
            Status = SubconOrderStatus.Approved;
        else
            throw new DomainException("Only submitted subcon orders can be approved.");
    }
    public void Reject()
    {
        if (Status == SubconOrderStatus.Submitted)
            Status = SubconOrderStatus.Rejected;
        else
            throw new DomainException("Only submitted subcon orders can be rejected.");
    }
    public void Return()
    {
        if (Status == SubconOrderStatus.Submitted)
            Status = SubconOrderStatus.Returned;
        else
            throw new DomainException("Only submitted subcon orders can be returned.");
    }
    public void Cancel()
    {
        if (Status == SubconOrderStatus.Submitted || (Status == SubconOrderStatus.Approved && !IsSentToSupplier))
        {
            Status = SubconOrderStatus.Cancelled;
        }
        else
            throw new DomainException("Only submitted or approved (not sent to supplier) subcon orders can be cancelled.");
    }
    public void MarkAsSentToSupplier()
    {
        if (Status == SubconOrderStatus.Approved)
            IsSentToSupplier = true;
        else
            throw new DomainException("Only approved subcon orders can be marked as sent to supplier.");
    }

    public void StartEditing()
    {
        if (Status == SubconOrderStatus.Returned)
            Status = SubconOrderStatus.Draft;
        else
            throw new DomainException("Only returned subcon orders can be edited.");
    }

    private bool IsAllowedToAddEditLine()
    {
        return Status == SubconOrderStatus.Draft || Status == SubconOrderStatus.Submitted
            || Status == SubconOrderStatus.Returned || Status == SubconOrderStatus.Approved;
    }

    public void AddLine(SubconOrderLine line)
    {
        if (!IsAllowedToAddEditLine())
            throw new DomainException("Cannot add line to subcon order in its current status.");

        Lines.Add(line);
        Status = SubconOrderStatus.Draft; // Reset status to Draft when a line is added
    }

    public void EditLine(int lineId, SubconOrderLine updatedLine)
    {
        if (!IsAllowedToAddEditLine())
            throw new DomainException("Cannot edit line in subcon order in its current status.");
        var existingLine = Lines.FirstOrDefault(l => l.Id == lineId);
        if (existingLine == null)
            throw new DomainException("Line not found.");
        existingLine.Update(updatedLine.MaterialId, updatedLine.QuantitySent, updatedLine.ExpectedOutputMaterialId, updatedLine.ExpectedOutputQuantity, updatedLine.ProcessingCostForeign);
        Status = SubconOrderStatus.Draft; // Reset status to Draft when a line is edited
    }

    public void RemoveLine(int lineId)
    {
        if (!IsAllowedToAddEditLine())
            throw new DomainException("Cannot remove line from subcon order in its current status.");
        var lineToRemove = Lines.FirstOrDefault(l => l.Id == lineId);
        if (lineToRemove == null)
            throw new DomainException("Line not found.");
        Lines.Remove(lineToRemove);
        Status = SubconOrderStatus.Draft; // Reset status to Draft when a line is removed
    }
}