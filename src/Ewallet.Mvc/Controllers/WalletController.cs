using Ewallet.Mvc.DTOs;
using Ewallet.Mvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Mvc.Controllers;

[ApiController]
[Route("api/v1/wallets")]

public sealed class WalletsController : ControllerBase
{
    private readonly IWalletService _service;

    public WalletsController(IWalletService service)
    {
        _service = service;   
    }   

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WalletResponse>> GetById([FromRoute] int userId, CancellationToken cancellationToken)
    {
        var wallet = await _service.GetByUserIdAsync(userId, cancellationToken);

        return wallet is null ? NotFound() : Ok(wallet);
    }


    [HttpPost("{userId}/topups")]
    [ProducesResponseType(typeof(TopUpResponse),StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TopUpResponse>> TopUp(
        [FromRoute]int userId, 
        [FromBody] TopUpRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var created = await _service.TopUpAsync( userId, request, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (ArgumentException e)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,title:"Invalid top up amount", detail:e.Message
            );
        }
         catch (InvalidOperationException e)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Wallet not found",
                detail: e.Message);
        }
    }


    [HttpPost("{userId}/transfers")]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransferResponse>> Transfer(
        [FromRoute] int userId,
        [FromBody] TransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.TransferAsync(
                userId,
                request,
                cancellationToken
            );

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid transfer request",
                detail: ex.Message
            );
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Transfer failed",
                detail: ex.Message
            );
        }
    }

}