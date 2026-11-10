using Ewallet.Mvc.DTOs;

namespace Ewallet.Mvc.Services;

public interface IUserService
{ 
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<UserResponse?> CreateAsync(UserRequest request, CancellationToken cancellationToken);
    Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);

    Task<List<TransactionHistoryResponse>> GetTransactionHistoryResponsesAsync(int userId,CancellationToken cancellationToken);
}