namespace ReconcileEngine.Api.Services.DTOs;

public record BatchProcessingResult(
    int BatchId,
    string FileName,
    string Status,
    int TotalRows,
    int ProcessedRows,
    int RejectedRows,
    int DiscrepanciesDetected);
