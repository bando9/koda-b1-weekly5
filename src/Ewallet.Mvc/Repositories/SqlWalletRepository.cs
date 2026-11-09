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

    public async Task<bool> TryTopUpAsync(int userId, decimal amount,SqlTransaction tx, CancellationToken cancellationToken)
    {

        const string sql = """
            UPDATE dbo.Wallets SET Balance = Balance + @Amount, Updated = GETDATE() WHERE UserId = @UserId;
        """;

        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection, tx);

        command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = amount;
        command.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;


        int rows = await command.ExecuteNonQueryAsync(cancellationToken);

        return rows >0;

        // const string walletSql = """
        //     UPDATE dbo.Wallets SET Balance = @Balance WHERE UserId = @Id
        // """;    

        // const string transactionSql = """
        //     INSERT INTO dbo.Transactions (ReceiverWalletId, Amount, TransactionType, Description) VALUES
        //     (@ReceiverWalletId,@Amount,'@TransactionType','@Description');
        // """;

        
        // await using var connection = new SqlConnection(_connectionString);

        // await connection.OpenAsync(cancellationToken);

        // await using var transaction1 = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        // try
        // {
        //     await using var walletCommand = new SqlCommand(walletSql,connection,transaction1);

        //     // walletCommand.Parameters.Add(@)
        //     AddBalanceParams(walletCommand, wallet);

        //     await using var transactionCommand = new SqlCommand(transactionSql,connection,transaction1);

        //     AddTransactionParam(transactionCommand,transaction);

        //     await transaction1.CommitAsync(cancellationToken);

        //     return true;

            
        // }
        // catch (SqlException e) when (e.Number is 2601 or 2627)
        // {
        //     await transaction1.RollbackAsync(cancellationToken);
        //     return false;
        // }
        // catch
        // {
        //     await transaction1.RollbackAsync(cancellationToken);
        //     return false;
        // }
    }

    public async Task<decimal?> GetBalanceByUserIdAsync(int userId,  SqlTransaction tx, CancellationToken cancellationToken)
    {
        const string sql = "SELECT Balance FROM dbo.Wallets WHERE UserId = @userId;"; 
        await using var connection = new SqlConnection(_connectionString);

        using var cmd = new SqlCommand(sql, connection, tx);
        cmd.Parameters.Add("@userId", SqlDbType.Int).Value = userId;

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result != null ? Convert.ToDecimal(result) : null;
    }
    public async Task<bool> UpdateBalanceAsync(
        int userId, 
        decimal amount,  
        SqlConnection connection, 
        SqlTransaction transaction, 
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Wallets
            SET Balance = Balance + @amount,
                Updated = GETDATE()
            WHERE UserId = @userId;
            """;

        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add("@amount", SqlDbType.Decimal).Value = amount;
        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;

        int rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    // *=========== METHOD

    private static void AddBalanceParams(SqlCommand command, Wallet wallet)
    {
        command.Parameters.Add("@Balance",SqlDbType.Decimal).Value = wallet.Balance;
    }

    private static void AddTransactionParam(SqlCommand command, Transaction transaction)
    {
        command.Parameters.Add("@ReceiverWalletId", SqlDbType.Int).Value = transaction.ReceiverWalletId;

        command.Parameters.Add("@Amount", SqlDbType.Decimal).Value = transaction.Amount;

        command.Parameters.Add("@TransactionType", SqlDbType.NVarChar,20).Value = transaction.TransactionType;

        command.Parameters.Add("@Description", SqlDbType.NVarChar,150).Value = transaction.Description;
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


}