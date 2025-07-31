using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using WebApi.Data.Entities;

namespace WebApi.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CommunicationDbContext context)
    {
        if (await context.GlobalStatuses.AnyAsync())
            return; // Already seeded

        // Seed Global Statuses
        var globalStatuses = new List<GlobalStatus>
        {
            new() { StatusCode = "ReadyForRelease", DisplayName = "Ready for Release", Phase = StatusPhase.Creation, SortOrder = 1 },
            new() { StatusCode = "Released", DisplayName = "Released", Phase = StatusPhase.Creation, SortOrder = 2 },
            new() { StatusCode = "QueuedForPrinting", DisplayName = "Queued for Printing", Phase = StatusPhase.Production, SortOrder = 3 },
            new() { StatusCode = "Printed", DisplayName = "Printed", Phase = StatusPhase.Production, SortOrder = 4 },
            new() { StatusCode = "Inserted", DisplayName = "Inserted", Phase = StatusPhase.Production, SortOrder = 5 },
            new() { StatusCode = "WarehouseReady", DisplayName = "Warehouse Ready", Phase = StatusPhase.Production, SortOrder = 6 },
            new() { StatusCode = "Shipped", DisplayName = "Shipped", Phase = StatusPhase.Logistics, SortOrder = 7 },
            new() { StatusCode = "InTransit", DisplayName = "In Transit", Phase = StatusPhase.Logistics, SortOrder = 8 },
            new() { StatusCode = "Delivered", DisplayName = "Delivered", Phase = StatusPhase.Logistics, SortOrder = 9 },
            new() { StatusCode = "Returned", DisplayName = "Returned", Phase = StatusPhase.Logistics, SortOrder = 10 },
            new() { StatusCode = "Failed", DisplayName = "Failed", Phase = StatusPhase.Production, SortOrder = 11 },
            new() { StatusCode = "Cancelled", DisplayName = "Cancelled", Phase = StatusPhase.Creation, SortOrder = 12 },
            new() { StatusCode = "Expired", DisplayName = "Expired", Phase = StatusPhase.Logistics, SortOrder = 13 },
            new() { StatusCode = "Archived", DisplayName = "Archived", Phase = StatusPhase.Logistics, SortOrder = 14 }
        };

        context.GlobalStatuses.AddRange(globalStatuses);
        await context.SaveChangesAsync();

        // Seed Communication Types
        var communicationTypes = new List<CommunicationType>
        {
            new() { TypeCode = "EOB", DisplayName = "Explanation of Benefits", Description = "Documents explaining insurance claim processing" },
            new() { TypeCode = "EOP", DisplayName = "Explanation of Payment", Description = "Documents detailing payment information" },
            new() { TypeCode = "ID_CARD", DisplayName = "ID Card", Description = "Member identification cards" }
        };

        context.CommunicationTypes.AddRange(communicationTypes);
        await context.SaveChangesAsync();

        // Seed default status mappings for each type
        await SeedDefaultStatusMappings(context);
    }

    private static async Task SeedDefaultStatusMappings(CommunicationDbContext context)
    {
        var eobStatuses = new List<CommunicationTypeStatus>
        {
            new() { TypeCode = "EOB", StatusCode = "ReadyForRelease", Description = "EOB ready for release", SortOrder = 1 },
            new() { TypeCode = "EOB", StatusCode = "Released", Description = "EOB has been released", SortOrder = 2 },
            new() { TypeCode = "EOB", StatusCode = "QueuedForPrinting", Description = "EOB queued for printing", SortOrder = 3 },
            new() { TypeCode = "EOB", StatusCode = "Printed", Description = "EOB has been printed", SortOrder = 4 },
            new() { TypeCode = "EOB", StatusCode = "Shipped", Description = "EOB has been shipped", SortOrder = 5 },
            new() { TypeCode = "EOB", StatusCode = "Delivered", Description = "EOB has been delivered", SortOrder = 6 }
        };

        var idCardStatuses = new List<CommunicationTypeStatus>
        {
            new() { TypeCode = "ID_CARD", StatusCode = "ReadyForRelease", Description = "ID Card ready for production", SortOrder = 1 },
            new() { TypeCode = "ID_CARD", StatusCode = "Released", Description = "ID Card released to production", SortOrder = 2 },
            new() { TypeCode = "ID_CARD", StatusCode = "QueuedForPrinting", Description = "ID Card queued for printing", SortOrder = 3 },
            new() { TypeCode = "ID_CARD", StatusCode = "Printed", Description = "ID Card has been printed", SortOrder = 4 },
            new() { TypeCode = "ID_CARD", StatusCode = "Inserted", Description = "ID Card has been inserted into envelope", SortOrder = 5 },
            new() { TypeCode = "ID_CARD", StatusCode = "WarehouseReady", Description = "ID Card ready at warehouse", SortOrder = 6 },
            new() { TypeCode = "ID_CARD", StatusCode = "Shipped", Description = "ID Card has been shipped", SortOrder = 7 },
            new() { TypeCode = "ID_CARD", StatusCode = "InTransit", Description = "ID Card is in transit", SortOrder = 8 },
            new() { TypeCode = "ID_CARD", StatusCode = "Delivered", Description = "ID Card has been delivered", SortOrder = 9 }
        };

        context.CommunicationTypeStatuses.AddRange(eobStatuses);
        context.CommunicationTypeStatuses.AddRange(idCardStatuses);
        await context.SaveChangesAsync();
    }
}
