
using System.Data;
using Ewallet.Mvc.DTOs;
using Ewallet.Mvc.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Mvc.Repositories;


public class SqlUserRepository : IUserRepository
{
    private readonly string _connectionString;
    

    public SqlUserRepository(IConfiguration configuration)
    {
        _connectionString=configuration.GetConnectionString("Ewalletdb") ?? throw new InvalidOperationException("Connection string tidak ditemukan di appsettings atau blm configurasi di Program.cs.");
        
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, Username, FullName, Email, Photo, Phone
            FROM dbo.Users;
        """;

        var users = new List<User>();

        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while(await reader.ReadAsync(cancellationToken))
        {
            users.Add(ReadUser(reader));
        }
        return users;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
        SELECT Id, Username, FullName, Email, Photo, Phone FROM dbo.Users WHERE Id = @id;
        """;

        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddUserId(command, id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }
        return ReadUser(reader);
    }

    public async Task<bool> TryAddAsync(User user, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO dbo.Users (Username, PasswordHash, Email, FullName, Phone, Photo)
            VALUES (@username, @passwordHash, @email, @fullName, @phone, @photo);
        """;

        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddUserParameters(command, user);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

            return affectedRows == 1;
        }
        catch (SqlException e) when (e.Number is 2601 or 2627)
        {
            return false;
        }
        catch (SqlException e)
        {
            System.Console.WriteLine($"SQL Error tidak terduga saat menambah user: {e.Number}", e.Number);
            return false;
        }
    }

    public async Task<bool> TryDeleteAsync(int id, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM dbo.Users WHERE Id = @id;
        """;

        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddUserId(command, id);
        
        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        return affectedRows == 1;


    }

    public async Task<bool> TryUpdateAsync(User user, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Users SET
                Username = @Username,
                Email = @Email,
                FullName = @FullName,
                Phone = @Phone,
                Photo = @Photo
            WHERE Id = @Id;
            """;

        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        AddUserParameters(command, user);
        AddUserId(command, user.Id);
        
        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        return affectedRows == 1;

    }

    public async Task<int?> TryAddWithWalletAsync(User user, Wallet wallet, CancellationToken cancellationToken)
    {
        const string userSql = """
            INSERT INTO dbo.Users (Username, PasswordHash, Email, FullName, Phone, Photo)
            OUTPUT INSERTED.Id
            VALUES (@Username, @PasswordHash, @Email, @FullName, @Phone, @Photo);
        """;

        const string walletSql = """
            INSERT INTO dbo.Wallets (WalletNumber, Balance, UserId)
            VALUES (@WalletNumber, @Balance, @UserId);
        """;

        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using var userCommand = new SqlCommand(userSql,connection,transaction);

            AddUserParameters(userCommand,user);

            var newUserId = (int)(await userCommand.ExecuteScalarAsync(cancellationToken))!;

            await using var walletCommand = new SqlCommand(walletSql, connection,transaction);

            walletCommand.Parameters.Add("@WalletNumber", SqlDbType.NVarChar, 20).Value = wallet.WalletNumber;
            walletCommand.Parameters.Add("@Balance", SqlDbType.Decimal).Value = wallet.Balance;
            walletCommand.Parameters.Add("@UserId", SqlDbType.Int).Value = newUserId;
            await walletCommand.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return newUserId;
        }
        catch (SqlException e) when (e.Number is 2601 or 2627)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<List<TransactionHistoryResponse>> GetTransactionHistoryAsync(int userId, CancellationToken cancellationToken)
    {
        var history = new List<TransactionHistoryResponse>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = @"
            SELECT
                t.Id,
                t.TransactionType,
                t.Amount,
                t.Description,
                t.CreatedAt,
                w.Id AS UserWalletId,
                t.ReceiverWalletId
            FROM dbo.Transactions t
            JOIN dbo.Wallets w ON t.SenderWalletId = w.Id OR t.ReceiverWalletId = w.Id
            WHERE w.UserId = @UserId
            ORDER BY t.CreatedAt DESC;
        ";


        using var command = new SqlCommand(sql,connection);

        command.Parameters.AddWithValue("@UserId", userId);

        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while(await reader.ReadAsync(cancellationToken))
            {
                

                int userWalletId = reader.GetInt32(reader.GetOrdinal("UserWalletId"));

                bool isIncoming = !reader.IsDBNull(reader.GetOrdinal("ReceiverWalletId")) && 
                                    reader.GetInt32(reader.GetOrdinal("ReceiverWalletId")) == userWalletId;

                var transaction = new TransactionHistoryResponse(
                        TransactionId: reader.GetInt32(reader.GetOrdinal("Id")),
                        TransactionType: reader.GetString(reader.GetOrdinal("TransactionType")),
                        Amount: reader.GetDecimal(reader.GetOrdinal("Amount")),
                        Description: reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        CreatedAt: reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        IsIncoming: isIncoming
                    );

                    history.Add(transaction);
            }
        }

        return history;
    }

    // * ==================== METHOD ====================

    private static void AddUserParameters(SqlCommand command, User user)
    {
        command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = user.Username.Trim();
        command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 255).Value = user.PasswordHash;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = user.Email.Trim().ToLowerInvariant();
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = user.FullName.Trim();
        command.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = user.Phone.Trim();
        
        command.Parameters.Add("@Photo", SqlDbType.NVarChar, 255).Value = 
            string.IsNullOrWhiteSpace(user.Photo) ? DBNull.Value : user.Photo.Trim();
    }

    private static void AddUserId(SqlCommand command, int userId)
    {
        command.Parameters.Add("@Id", SqlDbType.Int).Value = userId;
    }

    private static User ReadUser(SqlDataReader reader)
{
    var photoOrdinal = reader.GetOrdinal("Photo");

    return new User(
        id: reader.GetInt32(reader.GetOrdinal("Id")),
        username: reader.GetString(reader.GetOrdinal("Username")),
        passwordHash: string.Empty,
        email: reader.GetString(reader.GetOrdinal("Email")),
        photo: reader.IsDBNull(photoOrdinal) ? string.Empty : reader.GetString(photoOrdinal),
        fullName: reader.GetString(reader.GetOrdinal("FullName")),
        phone: reader.GetString(reader.GetOrdinal("Phone"))
    );
}
}

