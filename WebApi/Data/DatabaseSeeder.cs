using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CommunicationDbContext context)
    {
        if (!await context.GlobalStatuses.AnyAsync())
        {
            var globalStatuses = new[]
            {
                new GlobalStatus { StatusCode = "Pending",            Phase = "General",    Notes = "Waiting to be processed" },
                new GlobalStatus { StatusCode = "ReadyForRelease",    Phase = "Creation",   Notes = "Ready for final review and release" },
                new GlobalStatus { StatusCode = "Released",           Phase = "Creation",   Notes = "Released to production" },
                new GlobalStatus { StatusCode = "QueuedForPrinting",  Phase = "Production", Notes = "Queued for print job" },
                new GlobalStatus { StatusCode = "Printed",            Phase = "Production", Notes = "Printed successfully" },
                new GlobalStatus { StatusCode = "Inserted",           Phase = "Production", Notes = "Inserted into envelope/package" },
                new GlobalStatus { StatusCode = "WarehouseReady",     Phase = "Production", Notes = "Ready in warehouse for logistics" },
                new GlobalStatus { StatusCode = "Shipped",            Phase = "Logistics",  Notes = "Shipped to recipient" },
                new GlobalStatus { StatusCode = "InTransit",          Phase = "Logistics",  Notes = "Currently in transit" },
                new GlobalStatus { StatusCode = "Delivered",          Phase = "Logistics",  Notes = "Delivered successfully" },
                new GlobalStatus { StatusCode = "Returned",           Phase = "Logistics",  Notes = "Returned to sender" },
                new GlobalStatus { StatusCode = "Failed",             Phase = "Error",      Notes = "Processing failed" },
                new GlobalStatus { StatusCode = "Cancelled",          Phase = "Error",      Notes = "Cancelled before completion" },
                new GlobalStatus { StatusCode = "Expired",            Phase = "Error",      Notes = "Expired before completion" },
                new GlobalStatus { StatusCode = "Archived",           Phase = "Archive",    Notes = "Archived for record keeping" }
            };
            context.GlobalStatuses.AddRange(globalStatuses);
            await context.SaveChangesAsync();
        }

        if (!await context.Types.AnyAsync())
        {
            var types = new List<Type>
            {
                new() { TypeCode = "EOB", DisplayName = "Explanation of Benefits", IsActive = true },
                new() { TypeCode = "EOP", DisplayName = "Explanation of Payment", IsActive = true },
                new() { TypeCode = "ID_CARD", DisplayName = "ID Card", IsActive = true }
            };
            context.Types.AddRange(types);
            await context.SaveChangesAsync();
        }

        if (!await context.Statuses.AnyAsync())
        {
            var types = await context.Types.Where(t => t.IsActive).ToListAsync();
            var globalStatuses = await context.GlobalStatuses.ToListAsync();

            // Allowed statuses per type
            var allowedStatusesMap = new Dictionary<string, List<string>>
            {
                ["EOB"] = ["Pending", "ReadyForRelease", "Released", "QueuedForPrinting", "Printed", "Shipped", "Delivered"],
                ["EOP"] = ["Pending", "QueuedForPrinting", "Printed", "Shipped", "Delivered"],
                ["ID_CARD"] = ["Pending", "ReadyForRelease", "Released", "QueuedForPrinting", "Printed", "Inserted", "WarehouseReady", "Shipped", "InTransit", "Delivered"]
            };

            var statuses = new List<Status>();

            foreach (var type in types)
            {
                if (!allowedStatusesMap.TryGetValue(type.TypeCode, out var allowedCodes))
                {
                    allowedCodes = globalStatuses.Select(g => g.StatusCode).ToList();
                }

                foreach (var code in allowedCodes)
                {
                    var global = globalStatuses.FirstOrDefault(g => g.StatusCode == code);
                    if (global == null) continue;

                    statuses.Add(new Status
                    {
                        Id = Guid.NewGuid(),
                        TypeCode = type.TypeCode,
                        StatusCode = global.StatusCode,
                        Description = global.Notes,
                        IsActive = true,
                        GlobalStatus = global
                    });
                }
            }

            context.Statuses.AddRange(statuses);
            await context.SaveChangesAsync();
        }

        if (!await context.Communications.AnyAsync())
        {
            var types = await context.Types.Where(t => t.IsActive).ToListAsync();
            var communications = new List<Communication>();
            var rand = new Random();

            for (int i = 0; i < 20; i++)
            {
                var type = types[i % types.Count];
                var comm = new Communication
                {
                    Id = Guid.NewGuid(),
                    Title = $"{type.TypeCode} Sample #{i + 1}",
                    TypeCode = type.TypeCode,
                    CurrentStatusCode = "Pending",
                    LastUpdatedUtc = DateTime.UtcNow
                };
                communications.Add(comm);
            }

            context.Communications.AddRange(communications);
            await context.SaveChangesAsync();

            // Add random status history entries
            var statuses = await context.Statuses.Where(s => s.IsActive).ToListAsync();
            var histories = new List<StatusHistory>();

            foreach (var comm in communications)
            {
                int historyCount = rand.Next(1, 6);
                DateTime lastDate = comm.LastUpdatedUtc.AddDays(-historyCount);

                var allowedStatuses = statuses.Where(s => s.TypeCode == comm.TypeCode).ToList();

                for (int h = 0; h < historyCount; h++)
                {
                    var status = allowedStatuses[rand.Next(allowedStatuses.Count)];
                    lastDate = lastDate.AddDays(1);

                    histories.Add(new StatusHistory
                    {
                        Id = Guid.NewGuid(),
                        CommunicationId = comm.Id,
                        StatusCode = status.StatusCode,
                        OccurredUtc = lastDate
                    });
                }
            }

            context.StatusHistories.AddRange(histories);
            await context.SaveChangesAsync();
        }
    }
}
