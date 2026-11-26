using ReconcileEngine.Api.Domain.Enums;

namespace ReconcileEngine.Api.Domain.Entities;

public class Discrepancy
{
    public int Id { get; private set; }
    public int? OrderId { get; private set; }
    public int? StagingId { get; private set; }
    public DiscrepancyType DiscrepancyType { get; private set; }
    public decimal ExpectedAmount { get; private set; }
    public decimal ActualAmount { get; private set; }
    public decimal DifferenceAmount { get; private set; }
    public DateTime DetectedAt { get; private set; }

    private Discrepancy() { }

    public Discrepancy(int? orderId, int? stagingId, DiscrepancyType discrepancyType, decimal expectedAmount, decimal actualAmount)
    {
        if(orderId is null && stagingId is null)
            throw new ArgumentException("Either Order ID or Staging ID must be provided.");

        if(orderId <= 0)
            throw new ArgumentOutOfRangeException(nameof(orderId), "Order ID must be a positive integer.");

        if(stagingId <= 0)
            throw new ArgumentOutOfRangeException(nameof(stagingId), "Staging ID must be a positive integer.");

        if(!Enum.IsDefined(discrepancyType))
            throw new ArgumentOutOfRangeException(nameof(discrepancyType), "Invalid discrepancy type.");

        if (expectedAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedAmount), "Expected amount cannot be negative.");

        if(actualAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(actualAmount), "Actual amount cannot be negative.");

        OrderId = orderId;
        StagingId = stagingId;
        DiscrepancyType = discrepancyType;
        ExpectedAmount = expectedAmount;
        ActualAmount = actualAmount;
        DifferenceAmount = actualAmount - expectedAmount;
    }
}
