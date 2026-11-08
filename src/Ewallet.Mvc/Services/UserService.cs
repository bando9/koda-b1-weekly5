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
        
        var user = new User(0, request.Username, request.PasswordHash, request.Email, request.FullName, request.Phone, request.Photo ?? string.Empty);

        var created = await _repository.TryAddAsync(user, cancellationToken);

        if (!created)
        {
            return null;
        }

        return created ? UserResponse.FromModel(user) : null;
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
}