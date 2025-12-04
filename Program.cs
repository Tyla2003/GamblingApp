using GamblingApp.Data;
using GamblingApp.Services.Repositories;
using GamblingApp.Services;
using Microsoft.EntityFrameworkCore;

// Add services
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUserRepository, DbUserRepository>();
builder.Services.AddScoped<IGameRepository, DbGameRepository>();
builder.Services.AddScoped<IBetRepository, DbBetRepository>();
builder.Services.AddScoped<ITransactionRepository, DbTransactionRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, DbPaymentMethodRepository>();
builder.Services.AddScoped<IUserFavoriteRepository, DbUserFavoriteRepository>();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddSingleton<SlotRngService>();
builder.Services.AddSingleton<SlotPayoutService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

//Seeding DB 
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<GamblingApp.Data.DbSeeder>();
    await seeder.SeedAsync();
}

app.Run();
