using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace WebApi.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CommunicationDbContext context)
    {
        // 1️⃣ Seed CommunicationTypes
        if (!await context.CommunicationTypes.AnyAsync())
        {
            var types = new[]
            {
                new CommunicationType { TypeCode = "EOB",     DisplayName = "Explanation of Benefits" },
                new CommunicationType { TypeCode = "EOP",     DisplayName = "Explanation of Payment"  },
                new CommunicationType { TypeCode = "ID_CARD", DisplayName = "ID Card"                  }
            };
            context.CommunicationTypes.AddRange(types);
            await context.SaveChangesAsync();
        }

        // 2️⃣ Seed CommunicationTypeStatuses (including "Pending" first, and human-friendly descriptions)
        if (!await context.CommunicationTypeStatuses.AnyAsync())
        {
            var statusMap = new Dictionary<string, string[]>
            {
                ["EOB"] =
                [
                    "Pending",
                    "ReadyForRelease", "Released", "QueuedForPrinting", "Printed", "Shipped", "Delivered"
                ],
                ["EOP"] =
                [
                    "Pending",
                    "QueuedForPrinting", "Printed", "Shipped", "Delivered"
                ],
                ["ID_CARD"] =
                [
                    "Pending",
                    "ReadyForRelease", "Released", "QueuedForPrinting", "Printed",
                    "Inserted", "WarehouseReady", "Shipped", "InTransit", "Delivered"
                ]
            };

            var mappings = new List<CommunicationTypeStatus>();
            foreach (var (typeCode, codes) in statusMap)
            {
                foreach (var code in codes)
                {
                    // Split camel-case or underscores into words for Description
                    string description = Regex.Replace(
                        code,
                        "([a-z])([A-Z])",
                        "$1 $2"
                    ).Replace("_", " ");

                    mappings.Add(new CommunicationTypeStatus
                    {
                        Id          = Guid.NewGuid(),
                        TypeCode    = typeCode,
                        StatusCode  = code,
                        Description = description,
                        IsActive    = true
                    });
                }
            }

            context.CommunicationTypeStatuses.AddRange(mappings);
            await context.SaveChangesAsync();
        }

        // 3️⃣ Seed Communications + initial History (all start as "Pending")
        if (!await context.Communications.AnyAsync())
        {
            var typeCodes = new[] { "EOB", "EOP", "ID_CARD" };
            var communications = new List<Communication>();

            // create 10 communications, cycling through the three types
            for (int i = 0; i < 10; i++)
            {
                var typeCode = typeCodes[i % typeCodes.Length];
                communications.Add(new Communication
                {
                    Id             = Guid.NewGuid(),
                    Title          = $"{typeCode} Sample #{i + 1}",
                    TypeCode       = typeCode,
                    CurrentStatus  = "Pending",
                    LastUpdatedUtc = DateTime.UtcNow,
                    IsActive       = true
                });
            }

            context.Communications.AddRange(communications);

            // initial history entries
            var histories = communications.Select(c => new CommunicationStatusHistory
            {
                Id              = Guid.NewGuid(),
                CommunicationId = c.Id,
                StatusCode      = c.CurrentStatus,
                OccurredUtc     = c.LastUpdatedUtc,
                IsActive        = true
            });

            context.CommunicationStatusHistory.AddRange(histories);

            await context.SaveChangesAsync();
        }
    }
}
