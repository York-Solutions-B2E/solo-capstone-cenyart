using Shared.Dtos;

namespace Shared.Interfaces;

public interface ITokenTestService
{
    /// <summary>
    /// Gets a token using the client credentials flow (machine-to-machine authentication).
    /// </summary>
    /// <returns>A token response with access token and metadata if successful; otherwise null.</returns>
    Task<TokenTestResponse?> GetClientCredentialsTokenAsync();
}

