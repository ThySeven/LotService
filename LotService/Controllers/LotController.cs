using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LotService.Models;
using LotService.Services;

namespace LotService.Controllers;

[ApiController]
[Route("[controller]")]
public class LotController : ControllerBase
{

    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _config;
    private readonly ILotService _lotService;

    public LotController(ILogger<AuthController> logger, IConfiguration config, ILotService lotService)
    {
        _config = config;
        _logger = logger;
        _lotService = lotService;
    }


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

    [HttpPut]
    public async Task<IActionResult> UpdateLot([FromBody] LotModel lot)
    {
        if (lot == null)
        {
            return BadRequest();
        }
        try
        {
            await _lotService.UpdateLot(lot);
            return Ok(lot);
        }
        catch (Exception ex)
        {
            AuctionCoreLogger.Logger.Warn("Failed to create lot" + ex);
            return BadRequest();
        }
    }

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

    [HttpPut("close")]
    public async Task<IActionResult> CloseLot([FromBody] LotModel lot)
    {
        if (lot == null)
        {
            return BadRequest();
        }
        try
        {
            await _lotService.CloseLot(lot);
            return Ok(lot);
        }
        catch (Exception ex)
        {
            AuctionCoreLogger.Logger.Warn("Failed to create lot" + ex);
            return BadRequest();
        }
    }

}
