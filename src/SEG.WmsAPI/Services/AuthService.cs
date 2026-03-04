using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SEG.WmsAPI.Services;

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
}
