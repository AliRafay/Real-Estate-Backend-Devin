using System.Security.Claims;

namespace Demo.WebApi.Infrastructure.Auth;

public interface ICurrentUserInitializer
{
    void SetCurrentUser(ClaimsPrincipal user, string? token = null);

    void SetCurrentUserId(string userId);
}