using Demo.WebApi.Domain.Identity;

namespace Demo.WebApi.Application.Identity.Tokens;

public interface ITokenService : ITransientService
{
    Task<TokenResponseDto> GenerateTokensAndUpdateUserAsync(ApplicationUser user, string deviceId, string fcmToken, string appVersion, string deviceName);
    Task<TokenResponseDto> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken);
    Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequest request);
    Task<TokenResponseDto> VerifyBiometricsSignatureAsync(BiometricTokenRequest request);
}