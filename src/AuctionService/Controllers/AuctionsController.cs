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

    #region GET endpoints
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
            // .Select(x => MapAuctionDto(x))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null) return NotFound();

        return MapAuctionDto(auction);
    }
    #endregion

    #region POST endpoints
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionRequest rq)
    {
        if (rq is null) return BadRequest();

        var auction = new Auction
        {
            Status = Status.Live,
            ReservePrice = rq.ReservePrice,
            AuctionEnd = rq.AuctionEnd,
            Seller = "Test", // TODO: add current user as seller
            Item = new Item
            {
                Make = rq.Make,
                Model = rq.Model,
                Year = rq.Year,
                Color = rq.Color,
                Mileage = rq.Mileage,
                ImageUrl = rq.ImageUrl                
            }
        };

        await _context.Auctions.AddAsync(auction);

        bool result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Couldn't save changes to the DB");

        return CreatedAtAction(
            nameof(GetAuctionById),
            new { auction.Id },
            MapAuctionDto(auction)
        );
    }
    #endregion

    #region PUT endpoints
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionRequest rq)
    {
        if (rq is null) return BadRequest();

        var auction = await _context
            .Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null) return NotFound();

        auction.Item.Make = rq.Make;
        auction.Item.Model = rq.Model;
        auction.Item.Color = rq.Color;
        auction.Item.Mileage = rq.Mileage;
        auction.Item.Year = rq.Year;

        bool result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Problem saving changes.");

        return Ok();
    }
    #endregion

    #region DELETE endpoints
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context
            .Auctions
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction is null) return NotFound();

        _context.Remove(auction);
        
        bool result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Problem deleting auction.");

        return Ok();
    }
    #endregion

    #region Private
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
    #endregion
}
