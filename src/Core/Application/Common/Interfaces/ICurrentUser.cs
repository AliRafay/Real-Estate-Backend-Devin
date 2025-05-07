using System.Security.Claims;

namespace Demo.WebApi.Application.Common.Interfaces;

public interface ICurrentUser
{
    string? Name { get; }

    string GetUserId();

    string? GetUserEmail();

    bool IsAuthenticated();

    bool IsInRole(string role);

    string? GetToken();

    IEnumerable<Claim>? GetUserClaims();
}