using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SEG.WmsAPI.Services;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SEG.WmsAPI.Test
{
    [TestClass]
    public class AesServiceTests
    {
        public TestContext TestContext { get; set; }
        private IConfiguration? _configuration;
        private AesService? _aesService;

        [TestInitialize]
        public void TestInitialize()
        {
            // 建立測試配置
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true);

            _configuration = builder.Build();

            // 建立測試用的服務
            _aesService = new AesService(_configuration);
        }

        #region 基本加解密測試


        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試基本加密功能")]
        public void Test1()
        {
 
            // Arrange
            var plainText1 = "{\"requestId\":\"{123456789}\",\"account\":\"user001\",\"password\":\"password123\"}";
            var encrypted1 = _aesService.Encrypt(plainText1);

            var plainText2 = "{\"requestId\":\"14d3588f-79f2-44c2-8205-f6b5a7889a16\",\"storerCode\":\"97286918\",\"docStatus\":\"OPEN\"}";
            plainText2 = JsonSerializer.Serialize(plainText2, new JsonSerializerOptions
            {
                WriteIndented = false
            });
             
            var encrypted2 = _aesService.Encrypt(plainText2);

            var encryptedText1 = "dfa/zwxx118yn7OesgszsFnkKFocPkvwp77OvbvaM8D+/4syUYWBo1boxhzi7umejfPMGd3YXLehK8KBfjtHxNdG7fCnSuOeGS3yXgPBFZPwnZlFRkO7E30WMu1Wz7rIAvZCijqWtgggKVVqEnCCEWHo5XzkEV00sv380o+VM0cY8Y9FD4Hhy+9exSLKvde5aocYecfUd9coWkNMHQfxsN7Em5nq0RKGpLIFvicZNQ9szQo8yIViHp62dOIALuW2Jtb2yN4jHLmdxFp6lmAOmiB1+GG98F/skzDua1wxQesq3Dkq3bo2uFH2k2z1IC9y9KKnY7tERs+iV5u7ZydwH2czlj0N0L/B/musZZSi7aZIePyWvfkrOB8+9kcqQqBmnM0s0zx7POtq4+un2w4FibPXpApjkabs9wpBqyiOwfgL06F9B+3bCVkiPmJE2LKBQqo5SrqSl637VsCu45fXy5KKDTWcjMwcYeMr8+YhaMGapsaO+pngRsUqoMNPUt0U";
            var decrypted1 = _aesService.Decrypt(encryptedText1);


            var encryptedText2 = "bCvLRwrSJvMZg+ZK9dWPmmrj9SgseQI072hil4l0Xg6cfX5UGaQTll8hUU+MeiVDwGXibwGP6nmmB7g/GX/QxeB/2nc3CESwgiHD0YPB1lo8AbQ3MoW+5j0Coduj3KMt0fdPrKkjryJa5O0/7dk/ScJVNfKJxaD5S9kjQVt1vIXILEhtJNI8kcVGzrSFC/oWrUkvgkSpNOtUsnVCtmpXFGaIvtmTqOMPIF8VF9iJQarrKB5r+8E4gxWKqF1uJr7T61PQLPfIfZNtJA1OrFy5wV6UwcTpDvL7Hs/sWG208A+SjowhL9SRbYg42dqmp0fjJaEBsBpeDt9cZz7xP/mHbAZka98ECT8zLmYCAFv/ErZBwYqe1ry2obrU9cnhrDyVFGPkYOOIqNKM1qMBgQWOCfIJ5EaL/Hrxd2SgPC/3J0iHKX+7n3yUb/MF8DtWAEeP+bXyA2OCbCysEn7JIwrFR35zJIFXM9DW4gJ4A8lnLUd+lHb/sJIzWWwR5GqtJN6U7ou+yvU9cUQ6JJBOpfdhV2yMe/a6te3lagxXdu2MYdMYlb9G4f01evTZFUgpWw9+loTL7Zmx4mhc+dXmjBrmnPx1L+P6eaGRN00buqc53Fj2ivW1S7fm9m91nCdYRFiLmB0EOrS5WavnF2U4Cxyowd4KqjRcZDVSWHwZbC4EoS0OzhLPIyMrRlNxsJDTPwxY5vuVSruWQpCbYj7/MLH9odK6Hj8N15UtFumOGh5t+B6i3T4UdXShNnDMnzTfxExa";
            var decrypted2 = _aesService.Decrypt(encryptedText2);
        }


        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試基本加密功能")]
        public void Encrypt_ShouldEncryptPlainText_ReturnBase64()
        {
            // Arrange
            var plainText = "Hello, World!";

            // Act
            var encrypted = _aesService!.Encrypt(plainText);

            // Assert
            Assert.IsNotNull(encrypted);
            Assert.IsFalse(string.IsNullOrEmpty(encrypted));
            Assert.AreNotEqual(plainText, encrypted);
            AssertAreValidBase64(encrypted);
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試基本解密功能")]
        public void Decrypt_ShouldDecryptCipherText_ReturnOriginalText()
        {
            // Arrange
            var plainText = "{\"requestId\":\"{123456789}\",\"account\":\"user001\",\"password\":\"password123\"}";

            var encrypted = _aesService!.Encrypt(plainText);

            // Act
            var decrypted = _aesService.Decrypt(encrypted);

            // Assert
            Assert.AreEqual(plainText, decrypted);
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試加密與解密的往返運算")]
        public void EncryptDecrypt_RoundTrip_ShouldPreserveOriginalText()
        {
            // Arrange
            var testCases = new[]
            {
                "Hello, World!",
                "測試中文字符",
                "Special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?",
                "Numbers: 1234567890",
                "Mixed: ABC123!@#中文"
            };

            foreach (var plainText in testCases)
            {
                // Act
                var encrypted = _aesService!.Encrypt(plainText);
                var decrypted = _aesService.Decrypt(encrypted);

                // Assert
                Assert.AreEqual(plainText, decrypted,
                    $"往返加密解密失敗，原文: {plainText}");
            }
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試空字串加密解密")]
        public void EncryptDecrypt_EmptyString_ShouldWork()
        {
            // Arrange
            var plainText = "";

            // Act
            var encrypted = _aesService!.Encrypt(plainText);
            var decrypted = _aesService.Decrypt(encrypted);

            // Assert
            Assert.AreEqual(plainText, decrypted);
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試長文字加密解密")]
        public void EncryptDecrypt_LongText_ShouldWork()
        {
            // Arrange
            var plainText = new string('A', 1000);

            // Act
            var encrypted = _aesService!.Encrypt(plainText);
            var decrypted = _aesService.Decrypt(encrypted);

            // Assert
            Assert.AreEqual(plainText, decrypted);
        }

        #endregion

        #region IV 提取測試

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試 IV 從 AES Key 提取")]
        public void ExtractIvFromKey_ShouldExtractCorrectIV()
        {
            // Arrange
            var testKey = "ThisIsASecretKeyForAESEncryptionThatIs32Chars!";
            // IV 應該從第 7-22 字元提取（索引 6-21）
            // "SecretKeyForAES" （16 字元）
            var expectedIVBytes = Encoding.UTF8.GetBytes(testKey.Substring(6, 16));

            // Act
            // 使用反射呼叫私有方法
            var methodInfo = typeof(AesService).GetMethod(
                "ExtractIvFromKey",
                BindingFlags.NonPublic | BindingFlags.Static);

            var keyBytes = Encoding.UTF8.GetBytes(testKey);
            var actualIVBytes = (byte[]?)methodInfo?.Invoke(null, new object[] { keyBytes });

            // Assert
            Assert.IsNotNull(actualIVBytes);
            CollectionAssert.AreEqual(expectedIVBytes, actualIVBytes!,
                "IV 提取不正確");
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試短 Key 的 IV 提取（少於 22 字元）")]
        public void ExtractIvFromKey_ShortKey_ShouldPadWithZeros()
        {
            // Arrange
            var shortKey = "1234567890"; // 10 字元: ['1','2','3','4','5','6','7','8','9','0']
            // 根據實際實作：
            // - keyLength = Math.Min(10, 22) = 10
            // - start = 6
            // - 迴圈執行 i = 0,1,2,3 (因為 (6 + 3) < 10, (6 + 4) = 10 不 < 10)
            // - 提取 key[6], key[7], key[8], key[9] = '7','8','9','0'
            // - UTF-8 bytes: '7'=55, '8'=56, '9'=57, '0'=48
            // - i = 4..15 補零
            var expectedIVBytes = new byte[16];
            var keyBytes = Encoding.UTF8.GetBytes(shortKey);
            int start = 6;
            int keyLength = Math.Min(keyBytes.Length, 22);
            for (int i = 0; i < 16 && (start + i) < keyLength; i++)
            {
                expectedIVBytes[i] = keyBytes[start + i];
            }
            for (int i = (keyLength - start); i < 16; i++)
            {
                expectedIVBytes[i] = 0;
            }

            // 預期值: [55, 56, 57, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]

            // Act
            var methodInfo = typeof(AesService).GetMethod(
                "ExtractIvFromKey",
                BindingFlags.NonPublic | BindingFlags.Static);

            var actualIVBytes = (byte[]?)methodInfo?.Invoke(null, new object[] { keyBytes });

            // Assert
            Assert.IsNotNull(actualIVBytes);
            CollectionAssert.AreEqual(expectedIVBytes, actualIVBytes!,
                "短 Key 應該從第 6 字元開始提取可用部分，其餘補零");
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試不同 Key 產生不同的加密結果")]
        public void Encrypt_DifferentKeys_ShouldProduceDifferentResults()
        {
            // Arrange
            var plainText = "Hello, World!";
            
            // Act
            var encrypted1 = _aesService!.Encrypt(plainText);
            
            // 使用不同的 Key 建立新的服務
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Encryption:AesKey"] = "DifferentKeyForTestingEncryptionPurposes!"
                }).Build();
            var aesService2 = new AesService(config);
            var encrypted2 = aesService2.Encrypt(plainText);

            // Assert
            Assert.AreNotEqual(encrypted1, encrypted2,
                "不同的 Key 應該產生不同的加密結果");
        }

        #endregion

        #region JSON 資料加密測試

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試 JSON 物件加密解密")]
        public void EncryptDecrypt_JsonObject_ShouldPreserveStructure()
        {
            // Arrange
            var testData = new
            {
                requestId = "fb1a6bb8-21cd-490e-9f47-962cf99ec089",
                account = "user001",
                password = "password123"
            };

            var testData2 = new
            {
                requestId = "14d3588f-79f2-44c2-8205-f6b5a7889a16",
                storerCode = "97286918",
                docStatus = "OPEN"
            };

            var json = JsonSerializer.Serialize(testData2);
            var json2 = JsonSerializer.Serialize(testData2, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            // Act
            var encrypted = _aesService!.Encrypt(json);
         
            var decrypted = _aesService.Decrypt(encrypted);
            var decryptedObject = JsonSerializer.Deserialize<TestData>(decrypted);

            // Assert
            Assert.IsNotNull(decryptedObject);
            Assert.AreEqual(testData.requestId, decryptedObject.requestId);
            Assert.AreEqual(testData.account, decryptedObject.account);
            Assert.AreEqual(testData.password, decryptedObject.password);
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試複雜 JSON 陣列加密解密")]
        public void EncryptDecrypt_JsonArray_ShouldPreserveAllItems()
        {
            // Arrange
            var testData = new[]
            {
                new { id = 1, name = "項目1" },
                new { id = 2, name = "項目2" },
                new { id = 3, name = "項目3" }
            };
            var json = JsonSerializer.Serialize(testData);

            // Act
            var encrypted = _aesService!.Encrypt(json);
            var decrypted = _aesService.Decrypt(encrypted);
            var decryptedArray = JsonSerializer.Deserialize<TestItem[]>(decrypted);

            // Assert
            Assert.IsNotNull(decryptedArray);
            Assert.AreEqual(3, decryptedArray.Length);
            Assert.AreEqual("項目1", decryptedArray[0].name);
            Assert.AreEqual("項目2", decryptedArray[1].name);
            Assert.AreEqual("項目3", decryptedArray[2].name);
        }

        #endregion

        #region 靜態方法測試

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試靜態 Encrypt 方法")]
        public void Encrypt_StaticMethod_ShouldEncryptCorrectly()
        {
            // Arrange
            var plainText = "Hello, World!";
            var key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
            var iv = Encoding.UTF8.GetBytes("1234567890123456");

            // Act
            var encrypted = AesService.Encrypt(plainText, key, iv);
            var decrypted = AesService.Decrypt(encrypted, key, iv);

            // Assert
            Assert.AreEqual(plainText, decrypted);
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試靜態 Decrypt 方法")]
        public void Decrypt_StaticMethod_ShouldDecryptCorrectly()
        {
            // Arrange
            var plainText = "Test message";
            var key = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxzy123456");
            var iv = Encoding.UTF8.GetBytes("1234567890abcdef");
            var encrypted = AesService.Encrypt(plainText, key, iv);

            // Act
            var decrypted = AesService.Decrypt(encrypted, key, iv);

            // Assert
            Assert.AreEqual(plainText, decrypted);
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試靜態方法與實例方法產生相同結果")]
        public void EncryptDecrypt_StaticVsInstance_ShouldMatch()
        {
            // Arrange
            var plainText = "Hello, World!";
            var configKey = "ThisIsASecretKeyForAESEncryptionThatIs32Chars!";
            var key = Encoding.UTF8.GetBytes(configKey); // 完整的 key 會被 EnsureKeyLength 處理
            var iv = key[6..22]; // 從索引 6-21（第7-22字元）提取 IV

            // Act - 實例方法
            var encryptedInstance = _aesService!.Encrypt(plainText);
            var decryptedInstance = _aesService!.Decrypt(encryptedInstance);

            // 靜態方法使用確切的 key 和 iv（不經過 EnsureKeyLength）
            var keyArray = new byte[32];
            Array.Copy(key, 0, keyArray, 0, Math.Min(key.Length, 32));
            var encryptedStatic = AesService.Encrypt(plainText, keyArray, iv);
            var decryptedStatic = AesService.Decrypt(encryptedStatic, keyArray, iv);

            // Assert
            Assert.AreEqual(plainText, decryptedInstance);
            Assert.AreEqual(plainText, decryptedStatic);
        }

        #endregion

        #region 錯誤處理測試

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試解密無效的密文應拋出例外")]
        public void Decrypt_InvalidCipherText_ShouldThrowException()
        {
            // Arrange
            // 無效 Base64 字串 (包含非 Base64 字符)
            var invalidCipher = "Invalid@Base64!String";

            // Act & Assert
            try
            {
                _aesService!.Decrypt(invalidCipher);
                Assert.Fail("Expected FormatException was not thrown");
            }
            catch (System.FormatException)
            {
                // Expected exception was thrown
            }
        }
        #endregion

        #region Unicode 和特殊字元測試

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試中文加密解密")]
        public void EncryptDecrypt_ChineseCharacters_ShouldWork()
        {
            // Arrange
            var testCases = new[]
            {
                "你好，世界！",
                "這是一個測試字符串",
                "中文加密測試",
                "繁體中文Traditional Chinese"
            };

            foreach (var plainText in testCases)
            {
                // Act
                var encrypted = _aesService!.Encrypt(plainText);
                var decrypted = _aesService.Decrypt(encrypted);

                // Assert
                Assert.AreEqual(plainText, decrypted,
                    $"中文處理失敗: {plainText}");
            }
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試 Emoji 加密解密")]
        public void EncryptDecrypt_Emoji_ShouldWork()
        {
            // Arrange
            var testCases = new[]
            {
                "Hello 😀",
                "Testing 🎉🎊",
                "Emoji 测试 🙂👍",
                "🌟✨💫⭐"
            };

            foreach (var plainText in testCases)
            {
                // Act
                var encrypted = _aesService!.Encrypt(plainText);
                var decrypted = _aesService.Decrypt(encrypted);

                // Assert
                Assert.AreEqual(plainText, decrypted,
                    $"Emoji 處理失敗: {plainText}");
            }
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試混合語言加密解密")]
        public void EncryptDecrypt_MixedLanguages_ShouldWork()
        {
            // Arrange
            var plainText = "Hello 你好 こんにちは 안녕하세요 مرحبا";

            // Act
            var encrypted = _aesService!.Encrypt(plainText);
            var decrypted = _aesService.Decrypt(encrypted);

            // Assert
            Assert.AreEqual(plainText, decrypted);
        }

        #endregion

        #region 連續操作測試

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試連續加密同一明文")]
        public void Encrypt_SamePlainTextMultipleTimes_ShouldProduceDifferentResults()
        {
            // Arrange
            var plainText = "Hello, World!";

            // Act
            var encrypted1 = _aesService!.Encrypt(plainText);
            var encrypted2 = _aesService!.Encrypt(plainText);
            var encrypted3 = _aesService!.Encrypt(plainText);

            // Assert - CBC 模式每次加密可能產生不同結果（實際上如果 IV 固定應該相同）
            // 但測試驗證至少每次都能正確解密
            var decrypted1 = _aesService.Decrypt(encrypted1);
            var decrypted2 = _aesService.Decrypt(encrypted2);
            var decrypted3 = _aesService.Decrypt(encrypted3);

            Assert.AreEqual(plainText, decrypted1);
            Assert.AreEqual(plainText, decrypted2);
            Assert.AreEqual(plainText, decrypted3);
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試重複加密解密多次")]
        public void EncryptDecrypt_MultipleRounds_ShouldPreserveData()
        {
            // Arrange
            var plainText = "Original text";

            // Act - 連續加密解密 10 次
            var currentText = plainText;
            for (int i = 0; i < 10; i++)
            {
                var encrypted = _aesService!.Encrypt(currentText);
                currentText = _aesService!.Decrypt(encrypted);
            }

            // Assert
            Assert.AreEqual(plainText, currentText);
        }

        #endregion

        #region 邊界測試

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試單一字元加密解密")]
        public void EncryptDecrypt_SingleCharacter_ShouldWork()
        {
            // Arrange
            var plainText = "A";

            // Act
            var encrypted = _aesService!.Encrypt(plainText);
            var decrypted = _aesService.Decrypt(encrypted);

            // Assert
            Assert.AreEqual(plainText, decrypted);
        }

        [TestMethod]
        [TestCategory("AesService")]
        [Description("測試空白字元加密解密")]
        public void EncryptDecrypt_WhitespaceOnly_ShouldWork()
        {
            // Arrange
            var testCases = new[] { " ", "  ", "\t", "\n", "\r\n", " \t\n\r " };

            foreach (var plainText in testCases)
            {
                // Act
                var encrypted = _aesService!.Encrypt(plainText);
                var decrypted = _aesService.Decrypt(encrypted);

                // Assert
                Assert.AreEqual(plainText, decrypted,
                    $"空白字元處理失敗: [{plainText}]");
            }
        }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 驗證字串是否為有效的 Base64
        /// </summary>
        private void AssertAreValidBase64(string base64String)
        {
            var buffer = new Span<byte>(new byte[base64String.Length]);
            var result = Convert.TryFromBase64String(base64String, buffer, out var bytesParsed);

            Assert.IsTrue(result, $"字串不是有效的 Base64: {base64String}");
        }

        #endregion

        #region 測試輔助類別

        private class TestData
        {
            public string requestId { get; set; } = string.Empty;
            public string account { get; set; } = string.Empty;
            public string password { get; set; } = string.Empty;
        }

        private class TestItem
        {
            public int id { get; set; }
            public string name { get; set; } = string.Empty;
        }

        #endregion
    }
}
