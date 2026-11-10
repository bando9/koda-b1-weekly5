namespace Ewallet.Mvc.DTOs;

public record TransactionHistoryResponse(
    int TransactionId,string TransactionType,decimal Amount,string? Description,DateTime CreatedAt,bool IsIncoming
);