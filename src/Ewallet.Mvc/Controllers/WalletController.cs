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

            return created is null ? Problem(
                    statusCode: StatusCodes.Status404NotFound,title: "data not found"
                ) : Ok(created);
        }
        catch (ArgumentException e)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,title:"Invalid top up amount", detail:e.Message
            );
        }
    }
}