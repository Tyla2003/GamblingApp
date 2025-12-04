using GamblingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamblingApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Tables
    public DbSet<User> Users => Set<User>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Bet> Bets => Set<Bet>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<UserGameFavorite> UserGameFavorites => Set<UserGameFavorite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-many: User <-> Game through UserGameFavorite
        modelBuilder.Entity<UserGameFavorite>()
            .HasKey(ug => new { ug.UserId, ug.GameId });

        modelBuilder.Entity<UserGameFavorite>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.FavoriteGames)
            .HasForeignKey(ug => ug.UserId);

        modelBuilder.Entity<UserGameFavorite>()
            .HasOne(ug => ug.Game)
            .WithMany(g => g.FavoritedBy)
            .HasForeignKey(ug => ug.GameId);

        // User -> PaymentMethods (1 to many)
        modelBuilder.Entity<PaymentMethod>()
            .HasOne(pm => pm.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(pm => pm.UserId);

        // User -> Bets (1 to many)
        modelBuilder.Entity<Bet>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bets)
            .HasForeignKey(b => b.UserId);

        // Game -> Bets (1 to many)
        modelBuilder.Entity<Bet>()
            .HasOne(b => b.Game)
            .WithMany() 
            .HasForeignKey(b => b.GameId);

        // User -> Transactions (1 to many)
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId);

        // Bet -> Transactions (0/1 to many)
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Bet)
            .WithMany(b => b.Transactions)
            .HasForeignKey(t => t.BetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}