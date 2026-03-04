using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AesHelperGUI
{
    public partial class FormAes : Form
    {
        public FormAes()
        {
            InitializeComponent();
            SetDefaultValues();
        }

        /// <summary>
        /// 設定預設值
        /// </summary>
        private void SetDefaultValues()
        {
            // 設定預設 AES Key（從 appsettings.json 獲取或使用預設值）
            txtAesKey.Text = "ThisIsASecretKeyForSEGWmsAPITestOnly@DoNotUseItInPRODEnv!";
            
            // 設定預設輸入範例
            txtInput.Text = @"{
""requestId"": ""fb1a6bb8-21cd-490e-9f47-962cf99ec089"",
""account"": ""user001"",
""password"": ""password123""
}";
        }

        /// <summary>
        /// 執行按鈕點擊事件
        /// </summary>
        private void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                // 驗證輸入
                if (string.IsNullOrWhiteSpace(txtAesKey.Text))
                {
                    MessageBox.Show("請輸入 AES Key！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (txtAesKey.Text.Length < 22)
                {
                    MessageBox.Show("AES Key 長度至少需要 22 字元！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtInput.Text))
                {
                    MessageBox.Show("請輸入要加密或解密的 JSON 文本！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 進行加密或解密
                string result = rdoEncrypt.Checked ? ExecuteEncrypt() : ExecuteDecrypt();
                
                // 顯示結果
                txtOutput.Text = result;
                //MessageBox.Show("操作完成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                txtOutput.Text = string.Empty;
                MessageBox.Show($"操作失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 執行加密
        /// </summary>
        private string ExecuteEncrypt()
        {
            string plainText = txtInput.Text.Trim();
            
            // 1. Minimize JSON（移除多餘空白）
            string minimizedJson = MinimizeJson(plainText);
            
            // 2. 準備 AES Key (32 bytes) 和提取 IV (16 bytes)
            byte[] keyBytes = Encoding.UTF8.GetBytes(txtAesKey.Text);
            byte[] key = EnsureKeyLength(keyBytes);
            byte[] iv = ExtractIvFromKey(keyBytes);
            
            // 3. AES 256 加密
            string encryptedString = Encrypt(minimizedJson, key, iv);
            
            // 4. 構造外層請求包裝
            var outerRequest = new
            {
                RequestData = encryptedString
            };
            
            string resultJson = JsonSerializer.Serialize(outerRequest, new JsonSerializerOptions
            {
                WriteIndented = false
            });
            
            return resultJson;
        }

        /// <summary>
        /// 執行解密
        /// </summary>
        private string ExecuteDecrypt()
        {
            string inputText = txtInput.Text.Trim();
            
            // 1. 嘗試解析外層 JSON（可能已經是 {"RequestData": "..."} 格式）
            // 如果包含 "RequestData" 或 "ReturnData"，則提取加密內容
            string encryptedData = ExtractEncryptedData(inputText);
            
            // 2. 準備 AES Key (32 bytes) 和提取 IV (16 bytes)
            byte[] keyBytes = Encoding.UTF8.GetBytes(txtAesKey.Text);
            byte[] key = EnsureKeyLength(keyBytes);
            byte[] iv = ExtractIvFromKey(keyBytes);
            
            // 3. AES 256 解密
            string decryptedString = Decrypt(encryptedData, key, iv);
            
            return decryptedString;
        }

        /// <summary>
        /// 提取加密資料
        /// </summary>
        private string ExtractEncryptedData(string inputText)
        {
            // 嘗試解析為 JSON 並提取 RequestData 或 ReturnData
            try
            {
                using JsonDocument doc = JsonDocument.Parse(inputText);
                
                // 檢查是否有 RequestData
                if (doc.RootElement.TryGetProperty("RequestData", out var requestData))
                {
                    return requestData.GetString() ?? string.Empty;
                }
                
                // 檢查是否有 ReturnData
                if (doc.RootElement.TryGetProperty("ReturnData", out var returnData))
                {
                    return returnData.GetString() ?? string.Empty;
                }
                
                // 如果都沒有，假設輸入就是純 Base64 字串
                return inputText;
            }
            catch
            {
                // JSON 解析失敗，假設輸入就是純 Base64 字串
                return inputText;
            }
        }

        /// <summary>
        /// Minimize JSON（移除多餘空白）
        /// </summary>
        private string MinimizeJson(string json)
        {
            try
            {
                // 使用 JsonSerializer 重新序列化，WriteIndented = false 會自動 minimize
                using JsonDocument doc = JsonDocument.Parse(json);
                string minimized = JsonSerializer.Serialize(doc, new JsonSerializerOptions
                {
                    WriteIndented = false
                });
                return minimized;
            }
            catch
            {
                // JSON 解析失敗，直接返回原始字串
                return json;
            }
        }

        /// <summary>
        /// AES 256 加密
        /// </summary>
        private string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// AES 256 解密
        /// </summary>
        private string Decrypt(string cipherText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// 確保 AES Key 至少有 32 bytes (256 bits)
        /// </summary>
        private byte[] EnsureKeyLength(byte[] key)
        {
            byte[] fullKey = new byte[32];
            int copyLength = Math.Min(key.Length, 32);
            Array.Copy(key, 0, fullKey, 0, copyLength);
            return fullKey;
        }

        /// <summary>
        /// 從 AES Key 中提取 IV (第 7~22 字元，索引 6~21)
        /// </summary>
        private byte[] ExtractIvFromKey(byte[] key)
        {
            byte[] iv = new byte[16];
            int keyLength = Math.Min(key.Length, 22);
            int start = 6;
            
            for (int i = 0; i < 16 && (start + i) < keyLength; i++)
            {
                iv[i] = key[start + i];
            }
            
            // 如果 key 長度不足 22，用 0 補足
            for (int i = (keyLength - start); i < 16; i++)
            {
                iv[i] = 0;
            }
            
            return iv;
        }

        /// <summary>
        /// 清除按鈕點擊事件
        /// </summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtInput.Clear();
            txtOutput.Clear();
        }

        /// <summary>
        /// 複製按鈕點擊事件
        /// </summary>
        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtOutput.Text))
            {
                Clipboard.SetText(txtOutput.Text);
                MessageBox.Show("結果已複製到剪貼簿！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("沒有可複製的結果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
