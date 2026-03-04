namespace AesHelperGUI;

partial class FormAes
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        grpAesKey = new GroupBox();
        txtAesKey = new TextBox();
        grpOperation = new GroupBox();
        rdoDecrypt = new RadioButton();
        rdoEncrypt = new RadioButton();
        grpInput = new GroupBox();
        txtInput = new TextBox();
        btnClear = new Button();
        grpOutput = new GroupBox();
        txtOutput = new TextBox();
        btnCopy = new Button();
        btnExecute = new Button();
        grpOperation.SuspendLayout();
        grpInput.SuspendLayout();
        grpOutput.SuspendLayout();
        SuspendLayout();
        // 
        // grpAesKey
        // 
        grpAesKey.Font = new Font("微軟正黑體", 11F, FontStyle.Regular, GraphicsUnit.Point, 136);
        grpAesKey.Location = new Point(61, 12);
        grpAesKey.Name = "grpAesKey";
        grpAesKey.Size = new Size(706, 58);
        grpAesKey.TabIndex = 0;
        grpAesKey.TabStop = false;
        grpAesKey.Text = "AES Key";
        // 
        // txtAesKey
        // 
        txtAesKey.Location = new Point(61, 93);
        txtAesKey.Name = "txtAesKey";
        txtAesKey.PlaceholderText = "輸入 AES Key（至少 22 字元）";
        txtAesKey.Size = new Size(1481, 42);
        txtAesKey.TabIndex = 1;
        // 
        // grpOperation
        // 
        grpOperation.Controls.Add(rdoDecrypt);
        grpOperation.Controls.Add(rdoEncrypt);
        grpOperation.Font = new Font("微軟正黑體", 11F);
        grpOperation.Location = new Point(1570, 37);
        grpOperation.Name = "grpOperation";
        grpOperation.Size = new Size(314, 180);
        grpOperation.TabIndex = 1;
        grpOperation.TabStop = false;
        grpOperation.Text = "操作選擇";
        // 
        // rdoDecrypt
        // 
        rdoDecrypt.AutoSize = true;
        rdoDecrypt.Location = new Point(20, 108);
        rdoDecrypt.Name = "rdoDecrypt";
        rdoDecrypt.Size = new Size(245, 46);
        rdoDecrypt.TabIndex = 1;
        rdoDecrypt.Text = "解密 Decrypt";
        rdoDecrypt.UseVisualStyleBackColor = true;
        // 
        // rdoEncrypt
        // 
        rdoEncrypt.AutoSize = true;
        rdoEncrypt.Checked = true;
        rdoEncrypt.Location = new Point(20, 56);
        rdoEncrypt.Name = "rdoEncrypt";
        rdoEncrypt.Size = new Size(239, 46);
        rdoEncrypt.TabIndex = 0;
        rdoEncrypt.TabStop = true;
        rdoEncrypt.Text = "加密 Encrypt";
        rdoEncrypt.UseVisualStyleBackColor = true;
        // 
        // grpInput
        // 
        grpInput.Controls.Add(txtInput);
        grpInput.Controls.Add(btnClear);
        grpInput.Font = new Font("微軟正黑體", 12F);
        grpInput.Location = new Point(43, 236);
        grpInput.Name = "grpInput";
        grpInput.Size = new Size(2151, 504);
        grpInput.TabIndex = 2;
        grpInput.TabStop = false;
        grpInput.Text = "輸入 JSON";
        // 
        // txtInput
        // 
        txtInput.Font = new Font("微軟正黑體", 10F, FontStyle.Regular, GraphicsUnit.Point, 136);
        txtInput.Location = new Point(18, 54);
        txtInput.Multiline = true;
        txtInput.Name = "txtInput";
        txtInput.PlaceholderText = "輸入要加密或解密的 JSON 文本";
        txtInput.ScrollBars = ScrollBars.Vertical;
        txtInput.Size = new Size(2096, 423);
        txtInput.TabIndex = 1;
        // 
        // btnClear
        // 
        btnClear.Font = new Font("微軟正黑體", 11F);
        btnClear.Location = new Point(215, 0);
        btnClear.Name = "btnClear";
        btnClear.Size = new Size(280, 50);
        btnClear.TabIndex = 5;
        btnClear.Text = "清除 Clear";
        btnClear.UseVisualStyleBackColor = true;
        btnClear.Click += btnClear_Click;
        // 
        // grpOutput
        // 
        grpOutput.Controls.Add(txtOutput);
        grpOutput.Controls.Add(btnCopy);
        grpOutput.Font = new Font("微軟正黑體", 12F);
        grpOutput.Location = new Point(43, 778);
        grpOutput.Name = "grpOutput";
        grpOutput.Size = new Size(2151, 358);
        grpOutput.TabIndex = 3;
        grpOutput.TabStop = false;
        grpOutput.Text = "輸出結果";
        // 
        // txtOutput
        // 
        txtOutput.Font = new Font("微軟正黑體", 10F, FontStyle.Regular, GraphicsUnit.Point, 136);
        txtOutput.Location = new Point(18, 80);
        txtOutput.Multiline = true;
        txtOutput.Name = "txtOutput";
        txtOutput.PlaceholderText = "加密或解密結果將顯示在這裡";
        txtOutput.ReadOnly = true;
        txtOutput.ScrollBars = ScrollBars.Vertical;
        txtOutput.Size = new Size(2096, 249);
        txtOutput.TabIndex = 1;
        // 
        // btnCopy
        // 
        btnCopy.Font = new Font("微軟正黑體", 11F);
        btnCopy.Location = new Point(192, 0);
        btnCopy.Name = "btnCopy";
        btnCopy.Size = new Size(280, 50);
        btnCopy.TabIndex = 6;
        btnCopy.Text = "複製 Copy";
        btnCopy.UseVisualStyleBackColor = true;
        btnCopy.Click += btnCopy_Click;
        // 
        // btnExecute
        // 
        btnExecute.Font = new Font("微軟正黑體", 11F);
        btnExecute.Location = new Point(1890, 66);
        btnExecute.Name = "btnExecute";
        btnExecute.Size = new Size(280, 50);
        btnExecute.TabIndex = 4;
        btnExecute.Text = "執行 Execute";
        btnExecute.UseVisualStyleBackColor = true;
        btnExecute.Click += btnExecute_Click;
        // 
        // FormAes
        // 
        AutoScaleDimensions = new SizeF(16F, 35F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(2314, 1267);
        Controls.Add(txtAesKey);
        Controls.Add(btnExecute);
        Controls.Add(grpOutput);
        Controls.Add(grpInput);
        Controls.Add(grpOperation);
        Controls.Add(grpAesKey);
        Name = "FormAes";
        Text = "AES 加密/解密工具 - WMS API";
        grpOperation.ResumeLayout(false);
        grpOperation.PerformLayout();
        grpInput.ResumeLayout(false);
        grpInput.PerformLayout();
        grpOutput.ResumeLayout(false);
        grpOutput.PerformLayout();
        ResumeLayout(false);
        PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox grpAesKey;
    private System.Windows.Forms.TextBox txtAesKey;
    private System.Windows.Forms.GroupBox grpOperation;
    private System.Windows.Forms.RadioButton rdoEncrypt;
    private System.Windows.Forms.RadioButton rdoDecrypt;
    private System.Windows.Forms.GroupBox grpInput;
    private System.Windows.Forms.TextBox txtInput;
    private System.Windows.Forms.GroupBox grpOutput;
    private System.Windows.Forms.TextBox txtOutput;
    private System.Windows.Forms.Button btnExecute;
    private System.Windows.Forms.Button btnClear;
    private System.Windows.Forms.Button btnCopy;
}
