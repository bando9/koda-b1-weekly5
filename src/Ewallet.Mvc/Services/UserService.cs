using System.Security.Cryptography;
using Ewallet.Mvc.DTOs;
using Ewallet.Mvc.Models;
using Ewallet.Mvc.Repositories;

namespace Ewallet.Mvc.Services;

public sealed class UserService : IUserService
{

    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository=repository;    
    } 

    public async Task<UserResponse?> CreateAsync(UserRequest request, CancellationToken cancellationToken)
    {
        var user = new User(
            id:0,
            username:request.Username,passwordHash:request.PasswordHash,email:request.Email,photo:request.Photo ?? string.Empty,fullName:request.FullName,phone:request.Phone
        );

        var wallet = new Wallet(id:0,walletNumber:GenerateWalletNumber(),userId:0);

        var newUserId = await _repository.TryAddWithWalletAsync(user,wallet,cancellationToken);

        if(newUserId is null) return null;

        var created = new User(newUserId.Value,user.Username,user.PasswordHash,user.Email,user.Photo,user.FullName,user.Phone);

        return UserResponse.FromModel(created);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Task.FromResult(false);
        }
        return _repository.TryDeleteAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _repository.GetAllAsync(cancellationToken);

        return users.Select(UserResponse.FromModel).ToArray();
    }

    public async Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return null;
        }

        var user = await _repository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : UserResponse.FromModel(user);

    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await _repository.GetByIdAsync(id,cancellationToken);

        if(existingUser == null)
        {
            return null;
        }

        existingUser.UpdateProfile(request.FullName,request.Email,request.Phone,request.Photo);

        var updated = await _repository.TryUpdateAsync(existingUser, cancellationToken);

        return updated ? UserResponse.FromModel(existingUser) : null;
    }


    public async Task<List<TransactionHistoryResponse>> GetTransactionHistoryResponsesAsync(int userId,CancellationToken cancellationToken)
    {
        var transactions = await _repository.GetTransactionHistoryAsync(userId,cancellationToken);
        return transactions; 
    }

    private static string GenerateWalletNumber()
    {
        return string.Concat(Enumerable.Range(0, 12)
            .Select(_ => RandomNumberGenerator.GetInt32(0, 10)));
    }
}