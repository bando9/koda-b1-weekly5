namespace Ewallet.Mvc.Models;


public enum TransactionTypeEnum
{
    Income, 
    Expense,
    Transfer
}

public class Transaction {
    public int Id {get; private set;}
    public int? SenderWalletId {get; private set;} 
    public int ReceiverWalletId {get;private set;}
    public decimal Amount {get;private set;}
    public TransactionTypeEnum TransactionType {get; private set;}
    public string? Description {get;private set;}
    public DateTime CreatedAt {get;set;} = DateTime.Now;
    

    public Transaction(int id, int? senderWalletId,int receiverWalletId,decimal amount, TransactionTypeEnum transactionType, string? description)
    {
        Id = id;
        SenderWalletId = senderWalletId;
        ReceiverWalletId = receiverWalletId;
        Amount=amount;
        TransactionType=transactionType;
        Description=description;        
    }

    public Transaction(int? senderWalletId,int receiverWalletId,decimal amount, TransactionTypeEnum transactionType, string? description)
    {
        SenderWalletId = senderWalletId;
        ReceiverWalletId = receiverWalletId;
        Amount=amount;
        TransactionType=transactionType;
        Description=description;        
    }

}

