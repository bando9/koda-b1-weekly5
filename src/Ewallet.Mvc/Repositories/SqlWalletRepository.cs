using System.Data;
using Ewallet.Mvc.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Mvc.Repositories;

public class SqlWalletRepository : IWalletRepository
{
    private readonly string _connectionString;
 
    public SqlWalletRepository(IConfiguration configuration)
    {
        _connectionString=configuration.GetConnectionString("Ewalletdb") ?? throw new InvalidOperationException("Connection string tidak ditemukan di appsetting atau blm config di Program.cs");
    }

    public async Task<Wallet?> GetByUserId(int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, WalletNumber, Balance, UserId FROM dbo.Wallets WHERE UserId = @UserId
        """;

        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if(!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadWallet(reader);
    }


    public async Task<(int TransactionId, string WalletNumber, decimal NewBalance)> ProcessTopUpAsync(int userId, decimal amount, string? description, CancellationToken cancellationToken)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            int walletId  = 0;
            string walletNumber = string.Empty;
            decimal currentBalance = 0;

            string getWalletSql = "SELECT Id, WalletNumber,Balance FROM dbo.Wallets WHERE UserId = @UserId ";

            using var walletCommand = new SqlCommand(getWalletSql,connection,transaction);

            walletCommand.Parameters.AddWithValue("@UserId", userId);
            using (var reader = await walletCommand.ExecuteReaderAsync(cancellationToken))
            {                
                 if(await reader.ReadAsync(cancellationToken))
                {
                    walletId = reader.GetInt32(0);
                    walletNumber = reader.GetString(1);
                    currentBalance = reader.GetDecimal(2);
                } 
                else
                {
                    throw new Exception("wallet not found");    
                }
            }   

            string updateWalletQuery = @"
                UPDATE dbo.Wallets SET Balance = Balance + @Amount, Updated = @Now WHERE Id = @WalletId;
            ";

            using (var updateCommand = new SqlCommand(updateWalletQuery, connection, transaction))
            {
                
            
            updateCommand.Parameters.AddWithValue("@Amount", amount);
            updateCommand.Parameters.AddWithValue("@Now", DateTime.UtcNow);
            updateCommand.Parameters.AddWithValue("@WalletId", walletId);
            
            await updateCommand.ExecuteNonQueryAsync(cancellationToken);
            };
            
            string insertTxQuery = @"
                INSERT INTO dbo.Transactions (SenderWalletId, ReceiverWalletId, TransactionType,Amount, Description, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (NULL, @ReceiverWalletId, @TransactionType, @Amount, @Description, @Now)";
            
            int newTransactionId = 0;
            using (var cmd = new SqlCommand(insertTxQuery, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@ReceiverWalletId", walletId);
                cmd.Parameters.AddWithValue("@TransactionType", TransactionTypeEnum.Income);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Description", (object?)description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                
                newTransactionId = (int) await cmd.ExecuteScalarAsync(cancellationToken);
            }

            transaction.Commit();

            return (newTransactionId, walletNumber, currentBalance + amount);

        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    private static Wallet ReadWallet(SqlDataReader reader)
    {
        return new Wallet(
            id: reader.GetInt32(reader.GetOrdinal("Id")),
            walletNumber: reader.GetString(reader.GetOrdinal("WalletNumber")),
            balance: reader.GetDecimal(reader.GetOrdinal("Balance")),
            userId: reader.GetInt32(reader.GetOrdinal("UserId"))
        );
    }

}