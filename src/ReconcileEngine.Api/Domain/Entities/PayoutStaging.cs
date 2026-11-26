using ReconcileEngine.Api.Domain.Enums;

namespace ReconcileEngine.Api.Domain.Entities;

public class PayoutStaging
{
    public int Id { get; private set; }
    public int BatchId { get; private set; }
    public string ExternalTransactionId { get; private set; } = string.Empty;
    public string OrderCode { get; private set; } = string.Empty;
    public decimal NetAmount { get; private set; }
    public DateOnly PayoutDate { get; private set; }
    public StagingStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }

    private PayoutStaging() { }

    public PayoutStaging(int batchId, string externalTransactionId, string orderCode, decimal netAmount, DateOnly payoutDate)
    {
        if (batchId <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchId), "Batch ID must be greater than zero.");
        
        if (string.IsNullOrWhiteSpace(externalTransactionId))
            throw new ArgumentException("External transaction ID is required.", nameof(externalTransactionId));

        if (externalTransactionId.Trim().Length > 100)
            throw new ArgumentException("External transaction ID cannot exceed 100 characters.", nameof(externalTransactionId));

        if (string.IsNullOrWhiteSpace(orderCode))
            throw new ArgumentException("Order code is required.", nameof(orderCode));

        if(orderCode.Trim().Length > 50)
            throw new ArgumentException("Order code cannot exceed 50 characters.", nameof(orderCode));

        if(netAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(netAmount), "Net amount cannot be negative.");

        if (payoutDate == default)
            throw new ArgumentException("Payout date is required.", nameof(payoutDate));

        BatchId = batchId;
        ExternalTransactionId = externalTransactionId.Trim();
        OrderCode = orderCode.Trim();
        NetAmount = netAmount;
        PayoutDate = payoutDate;
        Status = StagingStatus.Pending;
    }

    public void MarkAsProcessed() 
    {
        Status = StagingStatus.Processed;
        RejectionReason = null;
    }

    public void Reject(string reason) 
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required.", nameof(reason));

        if(reason.Trim().Length > 255)
            throw new ArgumentException("Rejection reason cannot exceed 255 characters.", nameof(reason));

        Status = StagingStatus.Rejected;
        RejectionReason = reason.Trim();
    }
}