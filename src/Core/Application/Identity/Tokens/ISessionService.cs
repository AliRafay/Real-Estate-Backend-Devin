using Demo.WebApi.Domain.Common.Enums;
using Demo.WebApi.Domain.Identity;

namespace Demo.WebApi.Application.Tokens.Identity;

public interface ISessionService : IScopedService
{
    Task CreateSessionAsync(ApplicationUser user, DateTime expiry, string token, string deviceId, string fcmToken, string appVersion, string deviceName, SessionType sessionType = SessionType.Normal, bool isRemember = true);
    Task<UserSession> GetUserSessionAsync(string token);
    Task LogOutAllSessionsAsync(string userId);
    Task LogOutAllSessionsExceptCurrentUserAsync(string userId);
    Task LogOutSessionAsync(string token);
    Task<bool> VerifyTokenSessionAsync(string token);
}