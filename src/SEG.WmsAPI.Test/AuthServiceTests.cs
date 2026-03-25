using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEG.WmsAPI.Services;
using Microsoft.Extensions.Configuration;

namespace SEG.WmsAPI.Test
{
    [TestClass]
    public class AuthServiceTests
    {
        private IAuthService? _authService;

        [TestInitialize]
        public void TestInitialize()
        {
            // 建立測試配置
            var jwtSettings = new JwtSettings
            {
                Secret = "YourVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong!",
                Issuer = "SEG.WmsAPI",
                Audience = "SEG.WmsAPI",
                ExpiryHours = 24
            };

            _authService = new AuthService(jwtSettings);
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試生成 JWT Token")]
        public void GenerateToken_ValidAccount_ShouldReturnToken()
        {
            // Arrange
            string account = "testuser";

            // Act
            string token = _authService!.GenerateToken(account);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(token));
            Assert.IsTrue(token.Length > 0);
            Assert.IsTrue(token.Split('.').Length == 3); // JWT 格式: header.payload.signature
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試驗證有效的 JWT Token")]
        public void ValidateJwtToken_ValidToken_ShouldReturnValidResult()
        {
            // Arrange
            string account = "testuser";
            string token = _authService!.GenerateToken(account);

            // Act
            var result = _authService.ValidateJwtToken(token);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(account, result.Account);
            Assert.IsNotNull(result.Expiration);
            Assert.IsNotNull(result.IssuedAt);
            Assert.AreEqual("SEG.WmsAPI", result.Issuer);
            Assert.IsNull(result.ErrorMessage);
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試驗證空的 Token 應返回無效")]
        public void ValidateJwtToken_EmptyToken_ShouldReturnInvalid()
        {
            // Arrange
            string token = "";

            // Act
            var result = _authService!.ValidateJwtToken(token);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Token 為空", result.ErrorMessage);
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試驗證無效的簽章應返回無效")]
        public void ValidateJwtToken_InvalidSignature_ShouldReturnInvalid()
        {
            // Arrange
            // 使用格式正確但簽章錯誤的 Token
            string invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

            // Act
            var result = _authService!.ValidateJwtToken(invalidToken);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ErrorMessage?.Contains("簽章無效") == true || result.ErrorMessage?.Contains("驗證失敗") == true);
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試驗證錯誤格式的 Token 應返回無效")]
        public void ValidateJwtToken_MalformedToken_ShouldReturnInvalid()
        {
            // Arrange
            string malformedToken = "this.is.not.a.valid.jwt.token";

            // Act
            var result = _authService!.ValidateJwtToken(malformedToken);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試 Token 包含正確的 Claims")]
        public void ValidateJwtToken_ValidToken_ShouldContainCorrectClaims()
        {
            // Arrange
            string account = "testuser";
            string token = _authService!.GenerateToken(account);

            // Act
            var result = _authService.ValidateJwtToken(token);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsNotNull(result.Claims);
            if (result.Claims != null)
            {
                Assert.IsTrue(result.Claims.ContainsKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"));
                Assert.AreEqual(account, result.Claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);
            }
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試 Token 過期時間是否正確")]
        public void GenerateToken_ValidAccount_ShouldSetCorrectExpiration()
        {
            // Arrange
            string account = "testuser";
            var jwtSettings = new JwtSettings
            {
                Secret = "YourVeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong!",
                Issuer = "SEG.WmsAPI",
                Audience = "SEG.WmsAPI",
                ExpiryHours = 1
            };
            var authService = new AuthService(jwtSettings);

            // Act
            string token = authService.GenerateToken(account);
            var result = authService.ValidateJwtToken(token);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsNotNull(result.Expiration);
            var now = DateTime.UtcNow;
            var expiration = result.Expiration!.Value;

            // 驗證過期時間大約在 1 小時後（允許幾秒的誤差）
            var timeDifference = (expiration - now).TotalMinutes;
            Assert.IsTrue(timeDifference > 58 && timeDifference < 62);
        }

        #region 驗證帳號密碼測試

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試驗證帳號密碼 - 有效帳號密碼")]
        public void ValidateToken_ValidAccountAndPassword_ShouldReturnTrue()
        {
            // Arrange
            string account = "validuser";
            string password = "validpassword";

            // Act
            bool result = _authService!.ValidateUserAccount(account, password);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試驗證帳號密碼 - 空帳號")]
        public void ValidateToken_EmptyAccount_ShouldReturnFalse()
        {
            // Arrange
            string account = "";
            string password = "password";

            // Act
            bool result = _authService!.ValidateUserAccount(account, password);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試驗證帳號密碼 - 空密碼")]
        public void ValidateToken_EmptyPassword_ShouldReturnFalse()
        {
            // Arrange
            string account = "user";
            string password = "";

            // Act
            bool result = _authService!.ValidateUserAccount(account, password);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試驗證帳號密碼 - null 帳號")]
        public void ValidateToken_NullAccount_ShouldReturnFalse()
        {
            // Arrange
            string? account = null;
            string password = "password";

            // Act
            bool result = _authService!.ValidateUserAccount(account!, password);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [TestCategory("AuthService")]
        [Description("測試驗證帳號密碼 - null 密碼")]
        public void ValidateToken_NullPassword_ShouldReturnFalse()
        {
            // Arrange
            string account = "user";
            string? password = null;

            // Act
            bool result = _authService!.ValidateUserAccount(account, password!);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}