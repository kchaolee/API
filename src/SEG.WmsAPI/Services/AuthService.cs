using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SEG.WmsAPI.Services;

/// <summary>
/// Token 驗證結果
/// </summary>
public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public string? Account { get; set; }
    public DateTime? Expiration { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? Issuer { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string>? Claims { get; set; }
}

/// <summary>
/// JWT 設定
/// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryHours { get; set; }
}

/// <summary>
/// 加密設定
/// </summary>
public class EncryptionSettings
{
    public string AesKey { get; set; } = string.Empty;
}

/// <summary>
/// JWT 驗證服務介面
/// </summary>
public interface IAuthService
{
    string GenerateToken(string account);
    bool ValidateToken(string account, string password);
    TokenValidationResult ValidateJwtToken(string token);
}

/// <summary>
/// JWT 驗證服務實作
/// </summary>
public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;

    public AuthService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    /// <summary>
    /// 生成 JWT Token
    /// </summary>
    public string GenerateToken(string account)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: new[] { new Claim(ClaimTypes.Name, account) },
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 驗證帳號密碼
    /// </summary>
    public bool ValidateToken(string account, string password)
    {
        return !string.IsNullOrEmpty(account) && !string.IsNullOrEmpty(password);
    }

    /// <summary>
    /// 驗證 JWT Token 的有效性和正確性
    /// </summary>
    public TokenValidationResult ValidateJwtToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token 為空"
            };
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var jwtToken = validatedToken as JwtSecurityToken;
            var accountClaim = principal.FindFirst(ClaimTypes.Name);

            return new TokenValidationResult
            {
                IsValid = true,
                Account = accountClaim?.Value,
                Expiration = jwtToken?.ValidTo,
                IssuedAt = jwtToken?.ValidFrom,
                Issuer = jwtToken?.Issuer,
                Claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value)
            };
        }
        catch (SecurityTokenExpiredException)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token 已過期"
            };
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token 簽章無效"
            };
        }
        catch (SecurityTokenInvalidIssuerException)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token Issuer 無效"
            };
        }
        catch (SecurityTokenInvalidAudienceException)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token Audience 無效"
            };
        }
        catch (Exception ex)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Token 驗證失敗: {ex.Message}"
            };
        }
    }
}
