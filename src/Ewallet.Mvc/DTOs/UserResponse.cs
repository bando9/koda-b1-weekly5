using Ewallet.Mvc.Models;

namespace Ewallet.Mvc.DTOs;

public sealed record UserResponse(
    int Id,
    string Username ,
    string Email , 
    string Photo , 
    string FullName ,
    string Phone 
) 
{
    public static UserResponse FromModel(User user) => new(
        user.Id,
        user.Username,
        user.Email,
        user.Photo,
        user.FullName,
        user.Phone
    );
}