using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LotService.Models;
using LotService.Services;
using ZstdSharp.Unsafe;

namespace LotService.Controllers;

[ApiController]
[Route("[controller]")]
public class LotController : ControllerBase
{

    private readonly ILogger<LotController> _logger;
    private readonly IConfiguration _config;
    private readonly ILotService _lotService;

    public LotController(ILogger<LotController> logger, IConfiguration config, ILotService lotService)
    {
        _config = config;
        _logger = logger;
        _lotService = lotService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LotModel lot)
    {
        if (lot == null)
        {
            return BadRequest();
        }
        try
        {
            await _lotService.CreateLot(lot);
            return Ok(lot);
        }
        catch(Exception ex)
        {
            AuctionCoreLogger.Logger.Warn("Failed to create lot" + ex);
            return BadRequest();
        }
    }

    [Authorize]
    [HttpDelete("{lotId}")]
    public async Task<IActionResult> Delete(string lotId)
    {
        if (string.IsNullOrWhiteSpace(lotId))
        {
            return BadRequest();
        }
        try
        {
            await _lotService.DeleteLot(lotId);
            return Ok($"Lot with id {lotId} deleted");
        }
        catch (Exception ex)
        {
            AuctionCoreLogger.Logger.Warn($"Failed to delete lot with id {lotId}" + ex);
            return BadRequest();
        }
    }

    [HttpGet("{lotId}")]
    public async Task<IActionResult> GetLot(string lotId)
    {
        if (string.IsNullOrWhiteSpace(lotId))
        {
            return BadRequest();
        }
        try
        {
            LotModel lot = await _lotService.GetLot(lotId);
            return Ok(lot);
        }
        catch (Exception ex)
        {
            AuctionCoreLogger.Logger.Warn($"Failed to get lot with lotid: {lotId}" + ex);
            return BadRequest();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLots()
    {
        try
        {
            List<LotModel> lot = await _lotService.GetLots();
            return Ok(lot);
        }
        catch (Exception ex)
        {
            AuctionCoreLogger.Logger.Warn($"Failed to get all lots" + ex);
            return BadRequest();
        }
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateLot([FromBody] LotModel lot)
    {
        if (lot == null)
        {
            return BadRequest();
        }
        try
        {
            LotModel result = await _lotService.UpdateLot(lot);
            return Ok(result);
        }
        catch (Exception ex)
        {
            AuctionCoreLogger.Logger.Warn("Failed to create lot" + ex);
            return BadRequest();
        }
    }

    [Authorize]
    [HttpPut("bid")]
    public async Task<IActionResult> UpdateLotPrice([FromBody] BidModel bid)
    {
        if (bid == null)
        {
            return BadRequest();
        }
        try
        {
            await _lotService.UpdateLotPrice(bid);
            return Ok(bid);
        }
        catch (Exception ex)
        {
            AuctionCoreLogger.Logger.Warn("Failed to create lot" + ex);
            return BadRequest();
        }
    }

    [Authorize]
    [HttpPut("close")]
    public async Task<IActionResult> CloseLot([FromBody] string LotId)
    {
        if (LotId == null)
        {
            return BadRequest();
        }
        try
        {
            var result = await _lotService.CloseLot(LotId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            AuctionCoreLogger.Logger.Warn("Failed to create lot" + ex);
            return BadRequest();
        }
    }

    [Authorize]
    [HttpGet("/api/legal/auctions")]
    public async Task<IActionResult> GetAllLotsInteropability([FromQuery] DateTime? startDate)
    {
        var lotModels = await _lotService.GetLots();

        // Convert LotModel to Auction
        var auctions = lotModels.Select(AuctionMapper.MapToAuction).ToList();

        if (startDate.HasValue)
        {
            auctions = auctions.Where(a => a.StartDate >= startDate.Value).ToList();
        }

        if (!auctions.Any())
        {
            return NotFound(new { error = "No auctions found" });
        }

        return Ok(auctions);

    }

    [Authorize]
    [HttpGet("/api/legal/auctions/{auctionId}")]
    public async Task<IActionResult> GetAuctionById([FromRoute] Guid auctionId)
    {
        var lotModels = await _lotService.GetLots();
        var auction = lotModels
            .Select(AuctionMapper.MapToAuction)
            .FirstOrDefault(a => a.Id == auctionId);

        if (auction == null)
        {
            return NotFound(new { error = "Auction not found" });
        }

        return Ok(auction);
    }

}
