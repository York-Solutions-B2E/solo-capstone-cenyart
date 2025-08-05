using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApi.Data;

namespace WebApi.Swagger;

public class StatusOperationFilter(IServiceProvider provider) : IOperationFilter
{
    private readonly IServiceProvider _provider = provider;

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // find the parameter you want to populate
        var param = operation.Parameters?
            .FirstOrDefault(p => p.Name.Equals("newStatus", StringComparison.OrdinalIgnoreCase)
                              || p.Name.Equals("statusCode", StringComparison.OrdinalIgnoreCase));

        if (param == null)
            return;

        // create a scope so we can resolve the scoped DbContext
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();

        // load the live status codes
        var codes = db.GlobalStatuses
            .Where(gs => gs.IsActive)
            .OrderBy(gs => gs.SortOrder)
            .Select(gs => gs.StatusCode)
            .ToList();

        // inject into the OpenAPI parameter schema
        param.Schema.Enum = codes
            .Select(code => (IOpenApiAny)new OpenApiString(code))
            .ToList();

        param.Schema.Type = "string";
        param.Schema.Format = null;
    }
}
