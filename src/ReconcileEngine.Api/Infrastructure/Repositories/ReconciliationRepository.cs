using Dapper;
using ReconcileEngine.Api.Domain.Entities;
using ReconcileEngine.Api.Domain.Enums;
using ReconcileEngine.Api.Infrastructure.Data;
using System.Data;


namespace ReconcileEngine.Api.Infrastructure.Repositories;

public class ReconciliationRepository : IReconciliationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReconciliationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> BatchHashExistsAsync(string fileHash)
    {
        const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM payout_batches
                WHERE file_hash = @FileHash
            );   
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(sql, new { FileHash = fileHash });
    }

    public async Task<Contract?> GetContractByIdAsync(int contractId)
    {
        const string sql = """
            SELECT id, marketplace_name, commission_rate, payout_days, created_at
            FROM contracts
            WHERE id = @ContractId;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Contract>(sql, new {ContractId = contractId});
    }

    public async Task<IEnumerable<Order>> GetOrdersByContractIdAsync(int contractId)
    {
        const string sql = """
            SELECT id, contract_id, order_code, order_date, gross_amount, created_at
            from orders
            WHERE contract_id = @ContractId;
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Order>(sql, new { ContractId = contractId });
    }

    public async Task<int> InsertBatchAsync(PayoutBatch batch, IDbConnection connection, IDbTransaction transaction)
    {
        const string sql = """
            INSERT INTO payout_batches (contract_id, file_name, file_hash, status)
            VALUES (@ContractId, @FileName, @FileHash, @Status)
            RETURNING id;
            """;

        return await connection.ExecuteScalarAsync<int>(sql, batch, transaction);
    }

    public async Task<int> InsertStagingAsync(PayoutStaging staging, IDbConnection connection, IDbTransaction transaction)
    {
        const string sql = """
            INSERT INTO payout_staging (batch_id, external_transaction_id, order_code, net_amount, payout_date, status, rejection_reason)
            VALUES (@BatchId, @ExternalTransactionId, @OrderCode, @NetAmount, @PayoutDate, @Status, @RejectionReason)
            RETURNING id;
            """;

        return await connection.ExecuteScalarAsync<int>(sql, staging, transaction);
    }

    public async Task UpdateStagingStatusAsync(int stagingId, StagingStatus status, string? rejectReason, IDbConnection connection, IDbTransaction transaction)
    {
        const string sql = """
            UPDATE payout_staging
            SET status = @Status, 
                rejection_reason = @RejectionReason
            WHERE id = @Id;
            """;

        await connection.ExecuteAsync(sql, new {Id = stagingId, Status = status, RejectionReason = rejectReason }, transaction);
    }

    public async Task<int> InsertDiscrepancyAsync(Discrepancy discrepancy, IDbConnection connection, IDbTransaction transaction)
    {
        const string sql = """
            INSERT INTO discrepancies (order_id, staging_id, discrepancy_type, expected_amount, actual_amount, difference_amount)
            VALUES (@OrderId, @StagingId, @DiscrepancyType, @ExpectedAmount, @ActualAmount, @DifferenceAmount)
            RETURNING id;
            """;
        return await connection.ExecuteScalarAsync<int>(sql, discrepancy, transaction);
    }

    public async Task UpdateBatchStatusAsync(int batchId, BatchStatus status, IDbConnection connection, IDbTransaction transaction)
    {
        const string sql = """
            UPDATE payout_batches
            SET status = @Status
            WHERE id = @Id;
            """;

        await connection.ExecuteAsync(sql, new { Id = batchId, Status = status }, transaction);
    }
}
