using GamblingApp.Data;
using GamblingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamblingApp.Data
{
    public class DbSeeder
    {
        private readonly ApplicationDbContext _db;

        public DbSeeder(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SeedAsync()
        {
            await _db.Database.MigrateAsync();

            // ---------- USERS ----------
            if (!await _db.Users.AnyAsync())
            {
                var tyler = new User
                {
                    FirstName   = "Tyler",
                    LastName    = "Stephens",
                    Email       = "wtstephens1@gmail.com",
                    Password    = "tyler2",
                    DemoBalance = 10_000m,
                    IsAdmin     = true,
                    CreatedAt   = DateTime.UtcNow.AddDays(-7)
                };

                var drRoach = new User
                {
                    FirstName   = "Dr",
                    LastName    = "Roach",
                    Email       = "drroach@gmail.com",
                    Password    = "professor",
                    DemoBalance = 10_000m,
                    IsAdmin     = true,
                    CreatedAt   = DateTime.UtcNow  
                };

                var alice = new User
                {
                    FirstName   = "Alice",
                    LastName    = "Tester",
                    Email       = "alice@example.com",
                    Password    = "password1",
                    DemoBalance = 2_500m,
                    IsAdmin     = false,
                    CreatedAt   = DateTime.UtcNow.AddDays(-5)
                };

                var bob = new User
                {
                    FirstName   = "Bob",
                    LastName    = "Player",
                    Email       = "bob@example.com",
                    Password    = "password2",
                    DemoBalance = 1_000m,
                    IsAdmin     = false,
                    CreatedAt   = DateTime.UtcNow.AddDays(-3)
                };

                var carol = new User
                {
                    FirstName   = "Carol",
                    LastName    = "Gambler",
                    Email       = "carol@example.com",
                    Password    = "password3",
                    DemoBalance = 500m,
                    IsAdmin     = false,
                    CreatedAt   = DateTime.UtcNow.AddDays(-1)
                };

                _db.Users.AddRange(tyler, drRoach, alice, bob, carol);

                // ---------- GAMES ----------
                var slotsGame = new Game
                {
                    Name        = "Slots",
                    Description = "Classic 3-reel demo slot machine.",
                    MinBet      = 1m,
                    MaxBet      = 1000m,
                    IsEnabled   = true
                };

                var blackjackGame = new Game
                {
                    Name        = "Blackjack",
                    Description = "Standard demo Blackjack game vs dealer.",
                    MinBet      = 5m,
                    MaxBet      = 500m,
                    IsEnabled   = true   
                };

                _db.Games.AddRange(slotsGame, blackjackGame);

                // ---------- PAYMENT METHODS ----------
                var tylerCard = new PaymentMethod
                {
                    User            = tyler,
                    CardholderName  = "Tyler Stephens",
                    CardNumber      = "4111111111111111",
                    ExpMonth        = 5,
                    ExpYear         = 2030,
                    Nickname        = "Personal Visa",
                    Cvv             = "123",
                    IsActive        = true
                };

                _db.PaymentMethods.Add(tylerCard);

                // ---------- FAVORITES ----------
                _db.UserGameFavorites.Add(new UserGameFavorite
                {
                    User = tyler,
                    Game = slotsGame
                });

                await _db.SaveChangesAsync();
            }
        }
    }
}