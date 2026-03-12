using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TransactionIngestion.Data;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();

services.AddLogging(config =>
{
    config.AddConsole();
});

services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(configuration["Database:ConnectionString"]);
});

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

db.Database.EnsureCreated();

Console.WriteLine("Database initialized.");