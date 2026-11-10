namespace Ewallet.Mvc.DTOs;

public record TransferResponse(
    int TransactionId,
    string SenderWalletNumber,
    string ReceiverWalletNumber,
    decimal Amount,
    decimal SenderNewBalance,
    DateTime CreatedAt
);