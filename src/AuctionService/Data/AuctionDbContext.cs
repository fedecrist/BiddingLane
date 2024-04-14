using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext(DbContextOptions options) : DbContext(options)
{
    // Db Sets
    public DbSet<Auction> Auctions { get; set; }
    
}
