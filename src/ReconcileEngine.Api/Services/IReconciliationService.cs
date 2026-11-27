using ReconcileEngine.Api.Services.DTOs;

namespace ReconcileEngine.Api.Services;

public interface IReconciliationService
{
    Task<BatchProcessingResult> ProcessPayoutFileAsync(int contractId, string fileName, Stream fileStream);
}
