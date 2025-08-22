using Microsoft.AspNetCore.Authorization;
using Shared.Dtos;
using Shared.Interfaces;

namespace WebApi.GraphQL.User;

[ExtendObjectType("Query")]
[Authorize(Roles = "User")]
public class UserQuery
{
    public string Health() => "ok";

    [UsePaging(DefaultPageSize = 10, MaxPageSize = 50, IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<CommGraphDto> GetCommunications([Service] ICommService commService)
    {
        return commService.QueryCommunicationsGraph()
                          .OrderByDescending(c => c.LastUpdatedUtc)
                          .ThenBy(c => c.Id);
    }
}
