using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data
{
	public class AuctionDbContext : DbContext
	{
		public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options) { }

		public DbSet<Auction> Auctions { get; set; }
		public DbSet<Item> Items { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			
			modelBuilder.Entity<Auction>()
				.HasOne(a => a.Item)
				.WithOne(i => i.Auction)
				.HasForeignKey<Item>(i => i.AuctionId)
				.OnDelete(DeleteBehavior.Cascade);
			
			// Add three tables on Database - Outbox functionality
			modelBuilder.AddInboxStateEntity();
			modelBuilder.AddOutboxMessageEntity();
			modelBuilder.AddOutboxStateEntity();
		}
	}
}