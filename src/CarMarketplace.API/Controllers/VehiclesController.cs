using System.Security.Claims;
using CarMarketplace.Application.Abstractions;
using CarMarketplace.Application.DTOs.Common;
using CarMarketplace.Application.DTOs.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarMarketplace.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicles;

    public VehiclesController(IVehicleService vehicles) => _vehicles = vehicles;

    /// <summary>List vehicles with pagination and filters. Results are cached in Redis for repeated identical queries.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<VehicleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<VehicleResponse>>> List(
        [FromQuery] VehicleListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _vehicles.ListAsync(query, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<VehicleResponse>> Create(
        [FromBody] CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var sellerId))
            return Unauthorized();

        var created = await _vehicles.CreateAsync(sellerId, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(List), new { id = created.Id }, created);
    }
}
