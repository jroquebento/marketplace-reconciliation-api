namespace ReconcileEngine.Api.Domain.Entities;

public class Contract
{
    public int Id { get; private set; }
    public string MarketplaceName { get; private set; } = string.Empty;
    public decimal CommissionRate { get; private set; }
    public int PayoutDays { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Contract() { }

    public Contract(string marketplaceName, decimal commissionRate, int payoutDays)
    {
        if (string.IsNullOrWhiteSpace(marketplaceName))
            throw new ArgumentException("Marketplace name is required.", nameof(marketplaceName));

        if (marketplaceName.Trim().Length > 100)
            throw new ArgumentException("Marketplace name cannot exceed 100 characters.", nameof(marketplaceName));

        if (commissionRate is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(commissionRate), "Commission rate must be between 0 and 1.");

        if (payoutDays < 0)
            throw new ArgumentOutOfRangeException(nameof(payoutDays), "Payout days cannot be negative.");

        MarketplaceName = marketplaceName.Trim();
        CommissionRate = commissionRate;
        PayoutDays = payoutDays;
    }

    public decimal CalculateExpectedNetAmount(decimal grossAmount)
        => Math.Round(grossAmount * (1 - CommissionRate), 2, MidpointRounding.AwayFromZero);
}
