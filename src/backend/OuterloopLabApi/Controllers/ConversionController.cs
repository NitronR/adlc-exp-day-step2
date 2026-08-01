using Microsoft.AspNetCore.Mvc;
using OuterloopLabApi.Exceptions;
using OuterloopLabApi.Model;
using OuterloopLabApi.Services;

namespace OuterloopLabApi.Controllers;

[ApiController]
public sealed class ConversionController : ControllerBase
{
    private readonly ConversionService _conversionService;

    public ConversionController(ConversionService conversionService)
    {
        _conversionService = conversionService;
    }

    [HttpPost("/api/conversions")]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ConversionResponse>> Convert([FromBody] ConversionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _conversionService.ConvertAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (UpstreamCurrencyProviderUnavailableException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Currency provider unavailable",
                Detail = "The currency conversion provider is currently unavailable. Please retry shortly.",
            });
        }
    }

    [HttpGet("/api/audits/{auditId}")]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversionResponse>> GetAudit(string auditId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _conversionService.GetAuditAsync(auditId, cancellationToken);
            return Ok(response);
        }
        catch (AuditNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Audit record not found",
                Detail = "No audit record exists for the provided identifier.",
            });
        }
    }
}
