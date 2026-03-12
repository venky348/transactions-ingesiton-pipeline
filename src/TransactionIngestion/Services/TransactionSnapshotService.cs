using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TransactionIngestion.Models;

namespace TransactionIngestion.Services;

public class TransactionSnapshotService
{
    private readonly IConfiguration _configuration;

    public TransactionSnapshotService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<TransactionSnapshotDto>> GetSnapshotAsync()
    {
        var filePath = _configuration["MockApi:SnapshotFile"];

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Mock transaction snapshot not found");

        var json = await File.ReadAllTextAsync(filePath);

        return JsonSerializer.Deserialize<List<TransactionSnapshotDto>>(json)
               ?? new List<TransactionSnapshotDto>();
    }
}