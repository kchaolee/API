using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SEG.WmsAPI.Data;
using SEG.WmsAPI.Models.Entities;
using SEG.WmsAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        //options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.JsonSerializerOptions.WriteIndented = false;
    });

// 註定 AES 加密服務
builder.Services.AddScoped<IAesService, AesService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 設定資料庫
var dbSettings = builder.Configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>()
                  ?? throw new InvalidOperationException("Database configuration not found");

builder.Services.AddScoped<WmsDbContext>(provider =>
{
    var connectionString = dbSettings.ConnectionString; // Replace with actual connection string
    var enableSensitiveDataLogging = true; // Adjust as needed
    var commandTimeout = 60; // Adjust as needed
    return new WmsDbContext(connectionString, enableSensitiveDataLogging, commandTimeout);
});


//builder.Services.AddScoped<WmsDbContext>(options =>
//{
//    options.UseSqlServer(dbSettings.ConnectionString, sqlOptions =>
//    {
//        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
//        sqlOptions.CommandTimeout(dbSettings.CommandTimeout);
//    });

//    if (dbSettings.EnableSensitiveDataLogging)
//    {
//        options.EnableSensitiveDataLogging();
//    }

//    if (dbSettings.EnableQueryCache)
//    {
//        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
//    }
//});

builder.Services.AddScoped<WmsDb>();

// 設定 JWT 認證
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                   ?? throw new InvalidOperationException("JWT configuration not found");
var encryptionSettings = builder.Configuration.GetSection("Encryption").Get<EncryptionSettings>()
                          ?? throw new InvalidOperationException("Encryption configuration not found");

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton(encryptionSettings);

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 設定 HTTP 請求管線
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 靜態檔案服務 - 提供 HTML 頁面
app.UseStaticFiles();

// Token 驗證中間件 - 檢查 Bearer Token 是否存在
app.UseMiddleware<SEG.WmsAPI.Middleware.TokenValidationMiddleware>();

// 加密處理中間體 - 處理回應加密包裝
app.UseMiddleware<SEG.WmsAPI.Middleware.EncryptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
