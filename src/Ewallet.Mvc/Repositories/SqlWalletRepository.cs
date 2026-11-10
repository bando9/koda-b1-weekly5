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

    public async Task<Wallet?> GetByWalletNumberAsync(
    string walletNumber,
    CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, WalletNumber, Balance, UserId
            FROM dbo.Wallets
            WHERE WalletNumber = @WalletNumber;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@WalletNumber", SqlDbType.NVarChar, 20)
            .Value = walletNumber;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return ReadWallet(reader);
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


    public async Task<(
    int TransactionId,
    string SenderWalletNumber,
    string ReceiverWalletNumber,
    decimal SenderNewBalance,
    DateTime CreatedAt
    )> ProcessTransferAsync(
        int senderUserId,
        string receiverWalletNumber,
        decimal amount,
        string? description,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var transaction =
            connection.BeginTransaction(IsolationLevel.Serializable);

        try
        {
            int senderWalletId = 0;
            int receiverWalletId = 0;

            string senderNumber = string.Empty;
            string receiverNumber = string.Empty;

            decimal senderBalance = 0;

            const string getWalletsSql = """
                SELECT Id, WalletNumber, Balance, UserId
                FROM dbo.Wallets WITH (UPDLOCK, HOLDLOCK)
                WHERE UserId = @SenderUserId
                OR WalletNumber = @ReceiverWalletNumber;
                """;

            await using (var command = new SqlCommand(
                getWalletsSql, connection, transaction))
            {
                command.Parameters.Add("@SenderUserId", SqlDbType.Int)
                    .Value = senderUserId;

                command.Parameters.Add("@ReceiverWalletNumber", SqlDbType.NVarChar, 20)
                    .Value = receiverWalletNumber;

                await using var reader =
                    await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    int walletId = reader.GetInt32(reader.GetOrdinal("Id"));
                    string walletNumber = reader.GetString(
                        reader.GetOrdinal("WalletNumber"));
                    decimal balance = reader.GetDecimal(
                        reader.GetOrdinal("Balance"));
                    int userId = reader.GetInt32(reader.GetOrdinal("UserId"));

                    if (userId == senderUserId)
                    {
                        senderWalletId = walletId;
                        senderNumber = walletNumber;
                        senderBalance = balance;
                    }

                    if (walletNumber == receiverWalletNumber)
                    {
                        receiverWalletId = walletId;
                        receiverNumber = walletNumber;
                    }
                }
            }

            if (senderWalletId == 0)
                throw new InvalidOperationException("Wallet pengirim tidak ditemukan.");

            if (receiverWalletId == 0)
                throw new ArgumentException("Wallet penerima tidak ditemukan.");

            if (senderWalletId == receiverWalletId)
                throw new ArgumentException("Tidak dapat transfer ke wallet sendiri.");

            if (senderBalance < amount)
                throw new InvalidOperationException("Saldo tidak mencukupi.");

            const string debitSql = """
                UPDATE dbo.Wallets
                SET Balance = Balance - @Amount,
                    Updated = @Now
                WHERE Id = @SenderWalletId
                AND Balance >= @Amount;
                """;

            await using (var command = new SqlCommand(
                debitSql, connection, transaction))
            {
                command.Parameters.Add("@Amount", SqlDbType.Decimal)
                    .Value = amount;

                command.Parameters["@Amount"].Precision = 18;
                command.Parameters["@Amount"].Scale = 2;

                command.Parameters.Add("@Now", SqlDbType.DateTime2)
                    .Value = DateTime.UtcNow;

                command.Parameters.Add("@SenderWalletId", SqlDbType.Int)
                    .Value = senderWalletId;

                int affectedRows =
                    await command.ExecuteNonQueryAsync(cancellationToken);

                if (affectedRows != 1)
                    throw new InvalidOperationException("Saldo tidak mencukupi.");
            }

            const string creditSql = """
                UPDATE dbo.Wallets
                SET Balance = Balance + @Amount,
                    Updated = @Now
                WHERE Id = @ReceiverWalletId;
                """;

            await using (var command = new SqlCommand(
                creditSql, connection, transaction))
            {
                command.Parameters.Add("@Amount", SqlDbType.Decimal)
                    .Value = amount;

                command.Parameters["@Amount"].Precision = 18;
                command.Parameters["@Amount"].Scale = 2;

                command.Parameters.Add("@Now", SqlDbType.DateTime2)
                    .Value = DateTime.UtcNow;

                command.Parameters.Add("@ReceiverWalletId", SqlDbType.Int)
                    .Value = receiverWalletId;

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            const string insertTransactionSql = """
                INSERT INTO dbo.Transactions
                    (SenderWalletId, ReceiverWalletId, Amount,
                    TransactionType, Description, CreatedAt)
                OUTPUT INSERTED.Id, INSERTED.CreatedAt
                VALUES
                    (@SenderWalletId, @ReceiverWalletId, @Amount,
                    @TransactionType, @Description, @CreatedAt);
                """;

            int transactionId;
            DateTime createdAt = DateTime.UtcNow;

            await using (var command = new SqlCommand(
                insertTransactionSql, connection, transaction))
            {
                command.Parameters.Add("@SenderWalletId", SqlDbType.Int)
                    .Value = senderWalletId;

                command.Parameters.Add("@ReceiverWalletId", SqlDbType.Int)
                    .Value = receiverWalletId;

                command.Parameters.Add("@Amount", SqlDbType.Decimal)
                    .Value = amount;

                command.Parameters["@Amount"].Precision = 18;
                command.Parameters["@Amount"].Scale = 2;

                command.Parameters.Add("@TransactionType", SqlDbType.NVarChar, 20)
                    .Value = "Transfer";

                command.Parameters.Add("@Description", SqlDbType.NVarChar, 150)
                    .Value = (object?)description ?? DBNull.Value;

                command.Parameters.Add("@CreatedAt", SqlDbType.DateTime2)
                    .Value = createdAt;

                await using var reader =
                    await command.ExecuteReaderAsync(cancellationToken);

                await reader.ReadAsync(cancellationToken);

                transactionId = reader.GetInt32(0);
                createdAt = reader.GetDateTime(1);
            }

            await transaction.CommitAsync(cancellationToken);

            return (
                transactionId,
                senderNumber,
                receiverNumber,
                senderBalance - amount,
                createdAt
            );
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}