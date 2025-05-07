using Demo.WebApi.Application.Common.Exceptions;
using Demo.WebApi.Application.Common.Interfaces;
using Demo.WebApi.Application.Common.Persistence;
using Demo.WebApi.Application.Tokens.Identity;
using Demo.WebApi.Domain.Common.Enums;
using Demo.WebApi.Domain.Identity;
using Demo.WebApi.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.WebApi.Infrastructure.Identity;
public class SessionService : ISessionService
{
    private readonly IRepository<UserSession> _userSessionRepository;
    private readonly IStringLocalizer<SessionService> _t;
    private readonly ICurrentUser _currentUser;
    public SessionService(IRepository<UserSession> userSessions, IStringLocalizer<SessionService> stringLocalizer, ICurrentUser currentUser)
    {
        _userSessionRepository = userSessions;
        _t = stringLocalizer;
        _currentUser = currentUser;
    }

    public async Task<UserSession> GetUserSessionAsync(string token)
    {
        return await this._userSessionRepository.GetAll().Where(us => us.Token == token).FirstOrDefaultAsync();
    }

    public async Task CreateSessionAsync(
            ApplicationUser user,
            DateTime expiry,
            string token,
            string deviceId,
            string fcmToken,
            string appVersion,
            string deviceName,
            SessionType sessionType = SessionType.Normal,
            bool isRemember = true)
    {
        var session = new UserSession
        {
            ApplicationUser = user,
            Token = token,
            ExpiryDate = expiry,
            DeviceId = deviceId,
            Type = sessionType,
            IsRemember = isRemember,
            Version = appVersion,
            FCMToken = fcmToken,
            DeviceName = deviceName
        };

        await _userSessionRepository.AddAsync(session);
    }

    public async Task<bool> VerifyTokenSessionAsync(string token)
    {
        var now = DateTime.UtcNow;
        var session = await _userSessionRepository.GetAll().FirstOrDefaultAsync(x => x.Token == token);

        if (session == null || session.ExpiryDate < now)
            return false;

        return true;
    }

    public async Task LogOutSessionAsync(string token)
    {
        token = token.Replace("Bearer", string.Empty).Trim();
        var session = await _userSessionRepository.GetAll().FirstOrDefaultAsync(x => x.Token == token);

        if (session == null)
            throw new NotFoundException(_t["Session not found"]);

        session.ExpiryDate = DateTime.UtcNow;
        await _userSessionRepository.UpdateAsync(session);
    }

    public async Task LogOutAllSessionsAsync(string userId)
    {
        var sessions = await _userSessionRepository.GetAll()
                                          .Where(x => x.ApplicationUserId == userId && x.ExpiryDate > DateTime.UtcNow)
                                          .ToListAsync();

        foreach (var session in sessions) session.ExpiryDate = DateTime.UtcNow;
        await _userSessionRepository.UpdateRangeAsync(sessions);
    }

    public async Task LogOutAllSessionsExceptCurrentUserAsync(string userId)
    {
        var sessions = await _userSessionRepository.GetAll()
                                            .Where(x => (x.ApplicationUserId == userId) && (x.ExpiryDate > DateTime.UtcNow) && (x.Token != _currentUser.GetToken()))
                                            .ToListAsync();

        foreach (var session in sessions) session.ExpiryDate = DateTime.UtcNow;
        await _userSessionRepository.UpdateRangeAsync(sessions);
    }
}
