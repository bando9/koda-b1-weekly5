using System.Data;
using Microsoft.Data.SqlClient;

namespace Ewallet.Mvc.Repositories;

public class SqlTransactionRepository : ITransactionRepository
{
    private readonly string _connectionString;

    public SqlTransactionRepository(IConfiguration configuration)
    {
        _connectionString=configuration.GetConnectionString("Ewalletdb") ?? throw new InvalidOperationException("Connection string tidak ditemukan di appsetting atau blm config di Program.cs");
    }

    // public async Task<bool> CreateTransactionAsync(int? senderWalletId, int receiverWalletId, decimal amount, string transactionType, string? Description,SqlTransaction tx, CancellationToken cancellationToken)
    // {
    //     const string sql = """
    //         INSERT INTO dbo.Transactions (SenderWalletId, ReceiverWalletId, Amount, TransactionType, Description)
    //         VALUES (@SenderWalletId, @ReceiverWalletId, @Amount, @TransactionType, @Description);
    //     """;

    //     await using var connection = new SqlConnection(_connectionString);

    //     await connection.OpenAsync(cancellationToken);

    //     using var command = new SqlCommand(sql,connection,tx);

    //     command.Parameters.Add("@senderWalletId", SqlDbType.Int).Value = (object?)senderWalletId ?? DBNull.Value;
    //     command.Parameters.Add("@receiverWalletId", SqlDbType.Int).Value = receiverWalletId;
    //     command.Parameters.Add("@amount", SqlDbType.Decimal).Value = amount;
    //     command.Parameters.Add("@transactionType", SqlDbType.NVarChar, 50).Value = transactionType;

    //     int rows = await command.ExecuteNonQueryAsync(cancellationToken);
    //     return rows > 0;

    // }

    public async Task<bool> CreateTransactionAsync(
        int? senderWalletId, 
        int receiverWalletId, 
        decimal amount, 
        string transactionType,
        string? Description,
        SqlTransaction tx, 
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Transactions (SenderWalletId, ReceiverWalletId, Amount, TransactionType, CreatedAt)
            VALUES (@senderWalletId, @receiverWalletId, @amount, @transactionType, GETDATE());
            """;

        
        await using var connection = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, connection, tx);

        cmd.Parameters.Add("@senderWalletId", SqlDbType.Int).Value = (object?)senderWalletId ?? DBNull.Value;
        cmd.Parameters.Add("@receiverWalletId", SqlDbType.Int).Value = receiverWalletId;
        
        var amountParam = cmd.Parameters.Add("@amount", SqlDbType.Decimal);
        amountParam.Value = amount;
        amountParam.Precision = 18;
        amountParam.Scale = 2;

        cmd.Parameters.Add("@transactionType", SqlDbType.NVarChar, 50).Value = transactionType.Trim();

        int rowsAffected = await cmd.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }
}