using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TransactionIngestion.Data;
using TransactionIngestion.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();

services.AddSingleton<IConfiguration>(configuration);

services.AddLogging(config =>
{
    config.AddConsole();
});

services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(configuration["Database:ConnectionString"]);
});

services.AddSingleton<TransactionSnapshotService>();

var serviceProvider = services.BuildServiceProvider();

using var scope = serviceProvider.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var snapshotService = scope.ServiceProvider.GetRequiredService<TransactionSnapshotService>();

db.Database.EnsureCreated();

Console.WriteLine("Database initialized.");

var snapshot = await snapshotService.GetSnapshotAsync();

Console.WriteLine($"Loaded {snapshot.Count} transactions from snapshot.");