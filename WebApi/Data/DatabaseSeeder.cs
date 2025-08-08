using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace WebApi.Data;

public static partial class DatabaseSeeder
{
    public static async Task SeedAsync(CommunicationDbContext context)
    {
        // 1️⃣ Seed GlobalStatuses (canonical list)
        if (!await context.GlobalStatuses.AnyAsync())
        {
            var allGlobals = new[]
            {
                // Creation
                new GlobalStatus { Code = "ReadyForRelease", DisplayName = SplitName("ReadyForRelease"), Phase = "Creation" },
                new GlobalStatus { Code = "Released",         DisplayName = SplitName("Released"),         Phase = "Creation" },

                // Production
                new GlobalStatus { Code = "QueuedForPrinting", DisplayName = SplitName("QueuedForPrinting"), Phase = "Production" },
                new GlobalStatus { Code = "Printed",           DisplayName = SplitName("Printed"),           Phase = "Production" },
                new GlobalStatus { Code = "Inserted",          DisplayName = SplitName("Inserted"),          Phase = "Production" },
                new GlobalStatus { Code = "WarehouseReady",    DisplayName = SplitName("WarehouseReady"),    Phase = "Production" },

                // Logistics
                new GlobalStatus { Code = "Shipped",   DisplayName = SplitName("Shipped"),   Phase = "Logistics" },
                new GlobalStatus { Code = "InTransit", DisplayName = SplitName("InTransit"), Phase = "Logistics" },
                new GlobalStatus { Code = "Delivered", DisplayName = SplitName("Delivered"), Phase = "Logistics" },
                new GlobalStatus { Code = "Returned",  DisplayName = SplitName("Returned"),  Phase = "Logistics" },

                // Others
                new GlobalStatus { Code = "Failed",    DisplayName = SplitName("Failed"),    Phase = "Other" },
                new GlobalStatus { Code = "Cancelled", DisplayName = SplitName("Cancelled"), Phase = "Other" },
                new GlobalStatus { Code = "Expired",   DisplayName = SplitName("Expired"),   Phase = "Other" },
                new GlobalStatus { Code = "Archived",  DisplayName = SplitName("Archived"),  Phase = "Other" },
                new GlobalStatus { Code = "Pending",   DisplayName = SplitName("Pending"),   Phase = "Other" }
            };

            await context.GlobalStatuses.AddRangeAsync(allGlobals);
            await context.SaveChangesAsync();
        }

        // 2️⃣ Seed Types (communication types) if missing
        if (!await context.Types.AnyAsync())
        {
            var types = new[]
            {
                new Type { TypeCode = "EOB",     DisplayName = "Explanation of Benefits", IsActive = true },
                new Type { TypeCode = "EOP",     DisplayName = "Explanation of Payment",  IsActive = true },
                new Type { TypeCode = "ID_CARD", DisplayName = "ID Card",                 IsActive = true }
            };

            await context.Types.AddRangeAsync(types);
            await context.SaveChangesAsync();
        }

        // 3️⃣ Seed Status (allowed-status links per Type) if missing
        if (!await context.Statuses.AnyAsync())
        {
            var statusMap = new Dictionary<string, string[]>
            {
                ["EOB"] = new[]
                {
                    "Pending",
                    "ReadyForRelease", "Released",
                    "QueuedForPrinting", "Printed",
                    "Shipped", "Delivered"
                },
                ["EOP"] = new[]
                {
                    "Pending",
                    "QueuedForPrinting", "Printed",
                    "Shipped", "Delivered"
                },
                ["ID_CARD"] = new[]
                {
                    "Pending",
                    "ReadyForRelease", "Released",
                    "QueuedForPrinting", "Printed",
                    "Inserted", "WarehouseReady",
                    "Shipped", "InTransit", "Delivered"
                }
            };

            var mappings = new List<Status>();
            foreach (var kv in statusMap)
            {
                var typeCode = kv.Key;
                foreach (var statusCode in kv.Value)
                {
                    mappings.Add(new Status
                    {
                        Id = Guid.NewGuid(),
                        TypeCode = typeCode,
                        StatusCode = statusCode,
                        Description = SplitName(statusCode),
                        IsActive = true
                    });
                }
            }

            await context.Statuses.AddRangeAsync(mappings);
            await context.SaveChangesAsync();
        }

        // 4️⃣ Seed Communications + initial StatusHistory (all start as "Pending") if none exist
        if (!await context.Communications.AnyAsync())
        {
            var typeCodes = new[] { "EOB", "EOP", "ID_CARD" };
            var communications = new List<Communication>();

            for (int i = 0; i < 20; i++)
            {
                var typeCode = typeCodes[i % typeCodes.Length];
                var comm = new Communication
                {
                    Id = Guid.NewGuid(),
                    Title = $"{typeCode} Sample #{i + 1}",
                    TypeCode = typeCode,
                    CurrentStatusCode = "Pending",
                    LastUpdatedUtc = DateTime.UtcNow
                };

                communications.Add(comm);
            }

            await context.Communications.AddRangeAsync(communications);

            var histories = communications.Select(c => new StatusHistory
            {
                Id = Guid.NewGuid(),
                CommunicationId = c.Id,
                StatusCode = c.CurrentStatusCode,
                OccurredUtc = c.LastUpdatedUtc
            });

            await context.StatusHistories.AddRangeAsync(histories);
            await context.SaveChangesAsync();
        }
    }

    static string SplitName(string code)
    {
        // turn "QueuedForPrinting" or "ID_CARD" into "Queued For Printing" / "ID CARD"
        var splitCamel = MyRegex().Replace(code, "$1 $2");
        return splitCamel.Replace("_", " ");
    }

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex MyRegex();
}
