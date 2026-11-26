using ReconcileEngine.Api.Domain.Enums;

namespace ReconcileEngine.Api.Domain.Entities;

public class PayoutBatch
{
    public int Id { get; private set; }
    public int ContractId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileHash { get; private set; } = string.Empty;
    public BatchStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PayoutBatch() { }

    public PayoutBatch(int contractId, string fileName, string fileHash)
    {
        if (contractId <= 0)
            throw new ArgumentOutOfRangeException(nameof(contractId), "Contract ID must be greater than zero.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));

        if (fileName.Trim().Length > 255)
            throw new ArgumentException("File name is too long.", nameof(fileName));

        if (string.IsNullOrWhiteSpace(fileHash))
            throw new ArgumentException("File hash is required.", nameof(fileHash));

        if (fileHash.Trim().Length > 64)
            throw new ArgumentException("File hash is too long.", nameof(fileHash));

        ContractId = contractId;
        FileName = fileName.Trim();
        FileHash = fileHash.Trim();
        Status = BatchStatus.Processing;
    }

    public void MarkAsCompleted() => Status = BatchStatus.Completed;
    public void MarkAsFailed() => Status = BatchStatus.Failed;
}
