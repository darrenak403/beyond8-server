using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Beyond8.Common.Security;
using Beyond8.Identity.Application.Dtos.Tokens;
using Beyond8.Identity.Application.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Beyond8.Identity.Application.Services.Implements;

public class TokenService : ITokenService
{
    private readonly JwtBearerConfigurationOptions _jwtOptions;
    private readonly SymmetricSecurityKey _signingKey;

    public TokenService(IOptions<JwtBearerConfigurationOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));

        if (string.IsNullOrWhiteSpace(_jwtOptions.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey cannot be null or empty");
        }

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
    }

    public TokenResponse GenerateTokens(TokenClaims tokenClaims)
    {
        var accessToken = GenerateAccessToken(tokenClaims);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }

    private string GenerateAccessToken(TokenClaims tokenClaims)
    {
        var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, tokenClaims.UserId.ToString()),
                new(JwtRegisteredClaimNames.Email, tokenClaims.Email),
                new(JwtRegisteredClaimNames.UniqueName, tokenClaims.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new("userId", tokenClaims.UserId.ToString()),
                new("userName", tokenClaims.UserName)
            };
        foreach (var role in tokenClaims.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
