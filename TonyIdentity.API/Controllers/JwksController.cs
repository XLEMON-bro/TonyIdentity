using Microsoft.AspNetCore.Mvc;
using TonyIdentity.Business.Abstractions;

namespace TonyIdentity.API.Controllers;

[ApiController]
[Route(".well-known/jwks.json")]
public class JwksController : ControllerBase
{
    private readonly ISigningKeyProvider _signingKeyProvider;

    public JwksController(ISigningKeyProvider signingKeyProvider)
    {
        _signingKeyProvider = signingKeyProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var keys = await _signingKeyProvider.GetPublicJwksAsync(cancellationToken);
        return Ok(new { keys });
    }
}
