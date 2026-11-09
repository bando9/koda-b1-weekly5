
using Ewallet.Mvc.DTOs;
using Ewallet.Mvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Mvc.Controllers;

[ApiController]
[Route("api/v1/users")]

public sealed class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }


    // *Get ALl Users
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken ct)
    {
        return Ok(await _service.GetAllAsync(ct));
    }


    // *Get user by Id
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var user = await _service.GetByIdAsync(id, cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }


    // *Create user & Wallet
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> RegisterWithWallet(
        [FromBody] UserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _service.CreateAsync(request, cancellationToken);

            if (created is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Registrasi gagal.",
                    detail: "Username atau email sudah digunakan.");
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }
        catch (ArgumentException e)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid user data!",
                detail: e.Message);
        }
    }

    // *Update user by id
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Update([FromRoute]int id, [FromBody]UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {           

            var updated = await _service.UpdateAsync(id, request, cancellationToken);

            return updated is null ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Produk gagal diubah.", detail: $"Produk dg ID {id} tidak ditemukan.") : Ok(updated);
        }
        catch (ArgumentException e)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid user data!",
                detail: e.Message);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute]int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "User not found",
            detail: $"No user was found for ID {id}.");
    }
}