using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController(AuctionDbContext context) : ControllerBase
{
    private readonly AuctionDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        List<AuctionDto> auctions = await _context
            .Auctions
            .AsNoTracking()
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .Select(x => MapAuctionDto(x))
            .ToListAsync();

        return auctions;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context
            .Auctions
            .AsNoTracking()
            .Include(x => x.Item)
            .Select(x => MapAuctionDto(x))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null) return NotFound();

        // To be defined
        return auction;
    }

    private static AuctionDto MapAuctionDto(Auction x) => new()
    {
        Id = x.Id,
        ReservePrice = x.ReservePrice,
        Seller = x.Seller,
        Winner = x.Winner,
        SoldAmount = x.SoldAmount,
        CurrentHighBid = x.CurrentHighBid,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
        AuctionEnd = x.AuctionEnd,
        Status = x.Status.ToString(),
        Make = x.Item.Make,
        Model = x.Item.Model,
        Year = x.Item.Year,
        Color = x.Item.Color,
        Mileage = x.Item.Mileage,
        ImageUrl = x.Item.ImageUrl
    };
}
