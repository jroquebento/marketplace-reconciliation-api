
using ReconcileEngine.Api.Domain.Exceptions;
using ReconcileEngine.Api.Infrastructure.Data;
using ReconcileEngine.Api.Infrastructure.Repositories;
using ReconcileEngine.Api.Services.DTOs;
using System.Security.Cryptography;
using System.Globalization;
using ReconcileEngine.Api.Domain.Entities;


namespace ReconcileEngine.Api.Services;

public class ReconciliationService : IReconciliationService
{
    private readonly IReconciliationRepository _repository;
    private readonly IDbConnectionFactory _connectionFactory;

    public ReconciliationService(IReconciliationRepository repository, IDbConnectionFactory connectionFactory)
    {
        _repository = repository;
        _connectionFactory = connectionFactory;
    }

    public async Task<BatchProcessingResult> ProcessPayoutFileAsync(int contractId, string fileName, Stream fileStream)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        byte[] fileBytes = memoryStream.ToArray();

        string fileHash = Convert.ToHexString(SHA256.HashData(fileBytes)).ToLowerInvariant();

        if (await _repository.BatchHashExistsAsync(fileHash))
            throw new DomainException($"Este arquivo já foi processado anteriormente (hash: {fileHash}).", 409);

        var contract = await _repository.GetContractByIdAsync(contractId);
        if (contract == null)
            throw new DomainException($"Contrato com ID {contractId} não encontrado.", 404);

        List<CsvPayoutRow> csvRows = ParseCsvRows(fileBytes);

        var orders = await _repository.GetOrdersByContractIdAsync(contractId);
        var ordersByCode = orders.ToDictionary(o => o.OrderCode, StringComparer.OrdinalIgnoreCase);

        var seenOrderCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var batch = new PayoutBatch(contractId, fileName, fileHash);
            int batchId = await _repository.InsertBatchAsync(batch, connection, transaction);

            throw new NotImplementedException("Loop das 4 regras e Commit serão adicionados na Microetapa 3B.");
        }
        catch
        { 
            transaction.Rollback();
            throw;
        }
    }

    private static List<CsvPayoutRow> ParseCsvRows(byte[] fileBytes)
    {
        var rows = new List<CsvPayoutRow>();

        using var stream = new MemoryStream(fileBytes);
        using var reader = new StreamReader(stream);

        string? header = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(header))
            throw new DomainException("O arquivo CSV está vazio ", 400);

        int lineNumber = 1;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] columns = line.Split(',');
            if (columns.Length != 4)
                throw new DomainException($"Linha {lineNumber} inválida: esperado 4 colunas, encontrado {columns.Length}.", 400);

            string externalTransactionId = columns[0].Trim();
            string orderCode = columns[1].Trim();

            if (!decimal.TryParse(columns[2].Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal netAmount))
                throw new DomainException($"Linha {lineNumber}: valor líquido '{columns[2]}' inválido.", 400);

            if (!DateOnly.TryParseExact(columns[3].Trim(),"yyyy-MM-dd",CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly payoutDate))
                throw new DomainException($"Linha {lineNumber}: data '{columns[3]}' inválida. Use o formato yyyy-MM-dd.", 400);

            rows.Add(new CsvPayoutRow(externalTransactionId, orderCode, netAmount, payoutDate));
        }

        if (rows.Count == 0)
            throw new DomainException("O arquivo CSV não possui linhas de dados para processar.", 400);

        return rows;
    }

    private sealed record CsvPayoutRow(
        string ExternalTransactionId,
        string OrderCode,
        decimal NetAmount,
        DateOnly PayoutDate);
}
