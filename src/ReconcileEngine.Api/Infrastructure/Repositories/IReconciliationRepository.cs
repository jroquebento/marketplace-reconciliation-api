using ReconcileEngine.Api.Domain.Entities;
using ReconcileEngine.Api.Domain.Enums;
using System.Data;

namespace ReconcileEngine.Api.Infrastructure.Repositories;

public interface IReconciliationRepository
{
    // Leitura
    Task<bool> BatchHashExistsAsync(string fileHash);
    Task<Contract?> GetContractByIdAsync(int contractId);
    Task<IEnumerable<Order>> GetOrdersByContractIdAsync(int contractId);

    // Escrita transacional
    Task<int> InsertBatchAsync(PayoutBatch batch, IDbConnection connection, IDbTransaction transaction);
    Task<int> InsertStagingAsync(PayoutStaging staging, IDbConnection connection, IDbTransaction transaction);
    Task UpdateStagingStatusAsync(int stagingId, StagingStatus status, string? rejectReason, IDbConnection connection, IDbTransaction transaction);
    Task<int> InsertDiscrepancyAsync(Discrepancy discrepancy, IDbConnection connection, IDbTransaction transaction);
    Task UpdateBatchStatusAsync(int batchId, BatchStatus status, IDbConnection connection, IDbTransaction transaction);
}