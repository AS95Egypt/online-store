using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence;

public sealed class GovernorateSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<GovernorateSeeder> _logger;

    public GovernorateSeeder(AppDbContext dbContext, ILogger<GovernorateSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Governorates.AnyAsync(cancellationToken))
        {
            return;
        }

        // Initial development placeholders; the later shipping-admin story owns the production rates.
        var standardFee = 50m;
        var remoteFee = 75m;
        var governorates = new[]
        {
            "Alexandria", "Aswan", "Asyut", "Beheira", "Beni Suef", "Cairo", "Dakahlia",
            "Damietta", "Faiyum", "Gharbia", "Giza", "Ismailia", "Kafr El Sheikh", "Luxor",
            "Matrouh", "Minya", "Monufia", "New Valley", "North Sinai", "Port Said",
            "Qalyubia", "Qena", "Red Sea", "Sharqia", "Sohag", "South Sinai", "Suez",
        };
        var remoteGovernorates = new HashSet<string>(
            ["Aswan", "Matrouh", "New Valley", "North Sinai", "South Sinai"],
            StringComparer.Ordinal);

        _dbContext.Governorates.AddRange(governorates.Select(name => new Governorate
        {
            Name = name,
            ShippingFee = remoteGovernorates.Contains(name) ? remoteFee : standardFee,
        }));
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {GovernorateCount} governorate shipping options.", governorates.Length);
    }
}
