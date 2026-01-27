using Beyond8.Identity.Application.Dtos.Tokens;

namespace Beyond8.Identity.Application.Services.Interfaces
{
    public interface ITokenService
    {
        public TokenResponse GenerateTokens(TokenClaims claims);
        public string GenerateRefreshToken();
    }
}
