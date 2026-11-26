namespace ReconcileEngine.Api.Domain.Entities;

public class Order
{
    public int Id { get; private set; }
    public int ContractId { get; private set; }
    public string OrderCode { get; private set; } = string.Empty;
    public DateOnly OrderDate { get; private set; }
    public decimal GrossAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Order() { }

    public Order(int contractId, string orderCode, DateOnly orderDate, decimal grossAmount)
    {
        if (contractId <= 0)
            throw new ArgumentOutOfRangeException(nameof(contractId), "Contract ID must be greater than zero.");

        if (string.IsNullOrWhiteSpace(orderCode))
            throw new ArgumentException("Order code is required.", nameof(orderCode));

        if (orderCode.Trim().Length > 50)
            throw new ArgumentException("Order code cannot exceed 50 characters.", nameof(orderCode));
        
        if(orderDate == default)
            throw new ArgumentException("Order date is required.", nameof(orderDate));

        if (grossAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(grossAmount), "Gross amount cannot be negative.");

        ContractId = contractId;
        OrderCode = orderCode.Trim();
        OrderDate = orderDate;
        GrossAmount = grossAmount;
    }
}
