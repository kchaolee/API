using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SEG.WmsAPI.Middleware;
using SEG.WmsAPI.Services;
using System.Text.Json;
using System.Net;
using Moq;

namespace SEG.WmsAPI.Test
{
    [TestClass]
    public class TokenValidationMiddlewareTests
    {
        private Mock<ILogger<TokenValidationMiddleware>>? _mockLogger;
        private Mock<RequestDelegate>? _mockNext;
        private TokenValidationMiddleware? _middleware;
        private DefaultHttpContext? _httpContext;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLogger = new Mock<ILogger<TokenValidationMiddleware>>();
            _mockNext = new Mock<RequestDelegate>();
            _middleware = new TokenValidationMiddleware(_mockNext.Object, _mockLogger.Object);
            _httpContext = new DefaultHttpContext();
        }

        #region 登入端點不需要 Token

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("登入端點不需要 Token 驗證")]
        public async Task InvokeAsync_LoginEndpoint_ShouldSkipValidation()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/Auth/SignInVerification";
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
            Assert.AreNotEqual(401, _httpContext.Response.StatusCode);
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("登入端點沒有也不應該返回錯誤")]
        public async Task InvokeAsync_LoginEndpointWithoutToken_ShouldPass()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/Auth/SignInVerification";
            _httpContext.Request.Headers.Clear(); // 清除所有 Header
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
            Assert.AreNotEqual(401, _httpContext.Response.StatusCode);
        }

        #endregion

        #region 缺少 Authorization Header

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("缺少 Authorization Header 應返回 401")]
        public async Task InvokeAsync_MissingAuthorizationHeader_ShouldReturn401()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoHeaderData";
            _httpContext.Request.Headers.Clear();

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            Assert.AreEqual(401, _httpContext.Response.StatusCode);
            Assert.IsFalse(_mockNext!.Invocations.Any(), "不應該呼叫 next middleware");
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("缺少 Authorization Header 應返回 F119 錯誤")]
        public async Task InvokeAsync_MissingAuthorizationHeader_ShouldReturnF119Error()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoHeaderData";
            _httpContext.Request.Headers.Clear();
            _httpContext.Response.Body = new MemoryStream();

            // Act
            await _middleware!.InvokeAsync(_httpContext);
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

            // Assert
            Assert.AreEqual(401, _httpContext.Response.StatusCode);

            var response = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<TestErrorResponse>(response);

            Assert.IsNotNull(errorResponse);
            Assert.AreEqual("F", errorResponse.status);
            Assert.AreEqual("失敗-驗證失效", errorResponse.message);
            Assert.IsNotNull(errorResponse.errors);
            Assert.IsTrue(errorResponse.errors.Count > 0);
            Assert.AreEqual("F119", errorResponse.errors[0].code);
        }

        #endregion

        #region Authorization Header 格式錯誤

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("Authorization Header 格式不正確應返回 401")]
        public async Task InvokeAsync_InvalidAuthorizationFormat_ShouldReturn401()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoHeaderData";
            _httpContext.Request.Headers.Authorization = "InvalidFormat token";

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            Assert.AreEqual(401, _httpContext.Response.StatusCode);
            Assert.IsFalse(_mockNext!.Invocations.Any());
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("Authorization Header 缺少 Bearer 前綴應返回 401")]
        public async Task InvokeAsync_NoBearerPrefix_ShouldReturn401()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoDetailData";
            _httpContext.Request.Headers.Authorization = "someRandomToken";

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            Assert.AreEqual(401, _httpContext.Response.StatusCode);
        }

        #endregion

        #region Token 為空

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("Bearer Token 為空應返回 401")]
        public async Task InvokeAsync_EmptyBearerToken_ShouldReturn401()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoReceivingItem";
            _httpContext.Request.Headers.Authorization = "Bearer ";

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            Assert.AreEqual(401, _httpContext.Response.StatusCode);
            Assert.IsFalse(_mockNext!.Invocations.Any());
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("Bearer Token 只有空白字符應返回 401")]
        public async Task InvokeAsync_BlankBearerToken_ShouldReturn401()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoVerifying";
            _httpContext.Request.Headers.Authorization = "Bearer   ";

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            Assert.AreEqual(401, _httpContext.Response.StatusCode);
        }

        #endregion

        #region 有效的 Token

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("有效 Token 應通過驗證")]
        public async Task InvokeAsync_ValidBearerToken_ShouldPassValidation()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoCfmReceipt";
            _httpContext.Request.Headers.Authorization = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test";
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
            Assert.AreNotEqual(401, _httpContext.Response.StatusCode);
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("R1 端點需要 Token")]
        public async Task InvokeAsync_R1Endpoint_RequiresToken()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoHeaderData";
            _httpContext.Request.Headers.Authorization = "Bearer valid_token";
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("R2 端點需要 Token")]
        public async Task InvokeAsync_R2Endpoint_RequiresToken()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoDetailData";
            _httpContext.Request.Headers.Authorization = "Bearer valid_token";
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("R3 端點需要 Token")]
        public async Task InvokeAsync_R3Endpoint_RequiresToken()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoReceivingItem";
            _httpContext.Request.Headers.Authorization = "Bearer valid_token";
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("R4 端點需要 Token")]
        public async Task InvokeAsync_R4Endpoint_RequiresToken()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoVerifying";
            _httpContext.Request.Headers.Authorization = "Bearer valid_token";
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("R5 端點需要 Token")]
        public async Task InvokeAsync_R5Endpoint_RequiresToken()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoCfmReceipt";
            _httpContext.Request.Headers.Authorization = "Bearer valid_token";
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        #endregion

        #region 大小寫不敏感測試

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("Bearer 小寫也可以")]
        public async Task InvokeAsync_BearerLowerCase_ShouldAccept()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoHeaderData";
            _httpContext.Request.Headers.Authorization = "bearer valid_token";
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("BEARER 大寫也可以")]
        public async Task InvokeAsync_BearerUpperCase_ShouldAccept()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoDetailData";
            _httpContext.Request.Headers.Authorization = "BEARER valid_token";
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        #endregion

        #region 複雜路徑測試

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("路徑包含 /Auth/ 應跳過驗證")]
        public async Task InvokeAsync_PathContainsAuth_ShouldSkipValidation()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/Auth/SomeEndpoint"; // 符合實際 Auth 路徑模式
            _httpContext.Request.Headers.Clear();
            _mockNext!.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
            Assert.AreNotEqual(401, _httpContext.Response.StatusCode);
        }

        #endregion

        #region 錯誤回應格式測試

        [TestMethod]
        [TestCategory("TokenValidationMiddleware")]
        [Description("錯誤回應應包含正確的 Content-Type")]
        public async Task InvokeAsync_NotAuthorized_ShouldReturnJsonContentType()
        {
            // Arrange
            _httpContext.Request.Path = "/wmService/v1/PO/PoHeaderData";
            _httpContext.Request.Headers.Clear();

            // Act
            await _middleware!.InvokeAsync(_httpContext);

            // Assert
            Assert.AreEqual("application/json", _httpContext.Response.ContentType);
        }

        #endregion

        #region 輔助類別

        private class TestErrorResponse
        {
            public string requestId { get; set; } = "";
            public string status { get; set; } = "";
            public string message { get; set; } = "";
            public object? data { get; set; }
            public List<TestErrorDetail> errors { get; set; } = new();
        }

        private class TestErrorDetail
        {
            public string code { get; set; } = "";
            public string message { get; set; } = "";
        }

        #endregion
    }
}
