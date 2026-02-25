const { Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
        HeadingLevel, AlignmentType, WidthType, BorderStyle, ShadingType,
        PageBreak, Header, UnderlineType } = require('docx');
const fs = require('fs');

const CONTENT_WIDTH = 9360;
const BORDER = { style: BorderStyle.SINGLE, size: 1, color: "D3D3D3" };
const TABLE_BORDERS = { top: BORDER, bottom: BORDER, left: BORDER, right: BORDER };

function createParagraph(text, bold = false, fontSize = 22, headingLevel = null) {
  const run = new TextRun({
    text: text || "",
    bold: bold,
    size: fontSize,
    font: "Arial",
    color: "000000"
  });

  if (headingLevel !== null) {
    return new Paragraph({
      text: text || "",
      heading: headingLevel,
      spacing: { before: 240, after: 120 }
    });
  }
  return new Paragraph({
    children: [run],
    spacing: { after: 120 }
  });
}

function createCodeBlock(text) {
  return new Paragraph({
    children: [
      new TextRun({
        text: text || "",
        font: "Courier New",
        size: 18,
        color: "000000"
      })
    ],
    spacing: { before: 120, after: 120 },
    indent: { left: 720 },
    shading: { fill: "F5F5F5", type: ShadingType.CLEAR }
  });
}

function createTable(headers, rows, columnWidths) {
  const totalWidth = columnWidths.reduce((sum, w) => sum + (Number(w) || 0), 0);

  const headerCells = headers.map((header, idx) => {
    const width = Number(columnWidths[idx]) || 1000;
    return new TableCell({
      width: { size: width, type: WidthType.DXA },
      shading: { fill: "D9E1F2", type: ShadingType.CLEAR },
      borders: TABLE_BORDERS,
      margins: { top: 80, bottom: 80, left: 120, right: 120 },
      children: [createParagraph(header, true, 20)]
    });
  });

  const headerRow = new TableRow({ children: headerCells });

  const dataRows = rows.map(row => {
    const tableCells = row.map((cell, idx) => {
      const width = Number(columnWidths[idx]) || 1000;
      return new TableCell({
        width: { size: width, type: WidthType.DXA },
        borders: TABLE_BORDERS,
        margins: { top: 80, bottom: 80, left: 120, right: 120 },
        children: [createParagraph(cell || "", false, 20)]
      });
    });
    return new TableRow({ children: tableCells });
  });

  const numericWidths = columnWidths.map(w => Number(w) || 1000);

  return new Table({
    width: { size: totalWidth, type: WidthType.DXA },
    columnWidths: numericWidths,
    rows: [headerRow, ...dataRows]
  });
}

function createSection(title) {
  return [
    createParagraph(title, true, 26)
  ];
}

const children = [];

children.push(
  new Paragraph({
    text: "機場 WMS API 技術規格書",
    heading: HeadingLevel.HEADING_1,
    spacing: { before: 2880, after: 480 }
  }),
  createParagraph("版本：v1.01"),
  createParagraph("更新日期：2024-12-10"),
  createParagraph("機場APP"),
  new Paragraph({ children: [new PageBreak()] })
);

children.push(...createSection("1. 改訂版本"));
children.push(createTable(
  ["版本", "日期", "內容", "修訂者"],
  [
    ["v1.01", "2024-12-10", "初始版本，定義進貨作業接口規格", "Daniel"]
  ],
  [1800, 2200, 3500, 1860]
), new Paragraph({ text: "" }));

children.push(...createSection("2. 服務項目"));
children.push(
  createTable(
    ["服務名稱", "說明"],
    [
      ["A1-登入驗證", "取得授權 Token"],
      ["R1-取得預期收貨清單", "取得未完成收貨的採購單清單"],
      ["R2-取得收貨明細資料", "依採購單取得收貨明細"],
      ["R3-回傳收貨資料", "新增/更新收貨資料"],
      ["R4-取得收貨核對明細", "取得收貨核對資料與儲位選單"],
      ["R5-回傳全收確認", "確認過帳並產生板標籤"]
    ],
    [2800, 6560]
  ),
  new Paragraph({ text: "" })
);

// A1 接口
children.push(...createSection("A1-登入驗證"));
children.push(createParagraph("接口資訊", true, 24));
children.push(createTable(
  ["項目", "內容"],
  [
    ["接口名稱", "登入驗證"],
    ["接口代號", "A1"],
    ["URL", "POST /wmService/v1/Login"],
    ["通訊方式", "POST"],
    ["請求格式", "JSON"],
    ["回應格式", "JSON"],
    ["提供方", "WMS"],
    ["調用方", "外部系統 (Android App)"]
  ],
  [1500, 7860]
), new Paragraph({ text: "" }));

children.push(createParagraph("功能說明", true, 24));
children.push(createParagraph("進行帳號密碼驗證，成功後回傳授權 Token 與過期時間，用於後續 API 請求的認證。"));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("請求參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "最大長度", "必填"],
  [
    ["1", "requestId", "請求編號 (GUID)", "String(36)", "36", "V"],
    ["2", "account", "帳號", "String(50)", "50", "V"],
    ["3", "password", "密碼", "String(50)", "50", "V"]
  ],
  [800, 1500, 2000, 1400, 1000, 666]
), new Paragraph({ text: "" }));

children.push(createParagraph("請求範例", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "account": "user001",\n  "password": "password123"\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "最大長度", "必填"],
  [
    ["1", "requestId", "請求編號", "String(36)", "36", "V"],
    ["2", "status", "API回應結果代碼", "String(1)", "-", "V"],
    ["3", "message", "API回應結果訊息", "String(255)", "255", "V"],
    ["4", "accessToken", "授權 Token", "String(128)", "128", "V"],
    ["5", "expires", "過期時間", "String(19)", "-", "V"]
  ],
  [800, 1500, 2000, 1400, 1000, 1600]
), new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (成功)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "S",\n  "message": "登入成功",\n  "data": {\n    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",\n    "expires": "2024-12-11 23:59:59"\n  },\n  "errors": null\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (失敗 - 帳號密碼錯誤)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "F",\n  "message": "登入失敗",\n  "data": null,\n  "errors": [\n    {\n      "code": "F982",\n      "message": "帳號或密碼錯誤"\n    }\n  ]\n}`));

// R1 接口
children.push(...createSection("R1-取得預期收貨清單"));
children.push(createParagraph("接口資訊", true, 24));
children.push(createTable(
  ["項目", "內容"],
  [
    ["接口名稱", "取得預期收貨清單"],
    ["接口代號", "R1"],
    ["URL", "POST /wmService/v1/PO/PoHeaderData"],
    ["通訊方式", "POST"],
    ["請求格式", "JSON"],
    ["回應格式", "JSON"],
    ["提供方", "WMS"],
    ["調用方", "外部系統 (Android App)"]
  ],
  [1500, 7860]
), new Paragraph({ text: "" }));

children.push(createParagraph("功能說明", true, 24));
children.push(createParagraph("取得未完成收貨的採購單清單，用戶從清單中選擇要收貨的採購單。僅回傳狀態為「OPEN」的採購單。"));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("請求參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "最大長度", "必填"],
  [
    ["1", "requestId", "請求編號 (GUID)", "String(36)", "36", "V"],
    ["2", "storerCode", "貨主代碼", "String(15)", "15", "V"],
    ["3", "docStatus", "單據狀態", "String(10)", "10", "V"]
  ],
  [800, 1500, 2000, 1400, 1000, 1600]
), new Paragraph({ text: "" }));

children.push(createParagraph("請求範例", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "storerCode": "97286918",\n  "docStatus": "OPEN"\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "最大長度", "必填"],
  [
    ["1", "requestId", "請求編號", "String(36)", "36", "V"],
    ["2", "status", "API回應結果代碼", "String(1)", "-", "V"],
    ["3", "message", "API回應結果訊息", "String(255)", "255", "V"],
    ["4", "headerList", "表頭清單", "Array", "-", "-"],
    ["5", "wmsAsnNumber", "WMS預期收貨單號", "String(10)", "10", "V"],
    ["6", "storerCode", "貨主代碼", "String(15)", "15", "V"],
    ["7", "externReceiptNumber", "採購單號", "String(30)", "30", "V"],
    ["8", "vendorName", "供應商名稱", "String(30)", "30", "V"]
  ],
  [700, 1400, 1900, 1300, 900, 1300, 1300, 1600]
), new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (成功 - 有資料)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "S",\n  "message": "成功，共3筆資料",\n  "data": {\n    "headerList": [\n      {\n        "warehouseCode": "1020",\n        "wmsAsnNumber": "0000000001",\n        "storerCode": "69512619",\n        "externReceiptNumber": "API21052600601",\n        "vendorName": "供應商名稱"\n      },\n      {\n        "warehouseCode": "1020",\n        "wmsAsnNumber": "0000000002",\n        "storerCode": "69512619",\n        "externReceiptNumber": "API21052600602",\n        "vendorName": "供應商名稱1"\n      }\n    ]\n  },\n  "errors": null\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (成功 - 無資料)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "S",\n  "message": "成功，共0筆資料",\n  "data": {\n    "headerList": []\n  },\n  "errors": null\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (失敗)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "F",\n  "message": "失敗-檢查的資料不存在",\n  "data": null,\n  "errors": [\n    {\n      "code": "F982",\n      "message": "不存在定義-單據狀態 CLOSED"\n    }\n  ]\n}`));

// R2 接口
children.push(...createSection("R2-取得收貨明細資料"));
children.push(createParagraph("接口資訊", true, 24));
children.push(createTable(
  ["項目", "內容"],
  [
    ["接口名稱", "取得收貨明細資料"],
    ["接口代號", "R2"],
    ["URL", "POST /wmService/v1/PO/PoDetailData"],
    ["通訊方式", "POST"],
    ["請求格式", "JSON"],
    ["回應格式", "JSON"],
    ["提供方", "WMS"],
    ["調用方", "外部系統 (Android App)"]
  ],
  [1500, 7860]
), new Paragraph({ text: "" }));

children.push(createParagraph("功能說明", true, 24));
children.push(createParagraph("依採購單號取得收貨明細，包含 PO 項次、貨號、品名、到期日、漁廠代碼等資訊。使用者根據這些明細逐項輸入收貨資料。"));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("請求參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "最大長度", "必填"],
  [
    ["1", "requestId", "請求編號 (GUID)", "String(36)", "36", "V"],
    ["2", "wmsAsnNumber", "WMS預期收貨單號", "String(10)", "10", "V"],
    ["3", "storerCode", "貨主代碼", "String(15)", "15", "V"],
    ["4", "externReceiptNumber", "採購單號", "String(30)", "30", "V"]
  ],
  [800, 1500, 2000, 1400, 1000, 1600]
), new Paragraph({ text: "" }));

children.push(createParagraph("請求範例", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "wmsAsnNumber": "0000000001",\n  "storerCode": "69512619",\n  "externReceiptNumber": "API21052600601"\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "最大長度", "必填"],
  [
    ["1", "requestId", "請求編號", "String(36)", "36", "V"],
    ["2", "status", "API回應結果代碼", "String(1)", "-", "V"],
    ["3", "message", "API回應結果訊息", "String(255)", "255", "V"],
    ["4", "detailList", "明細資料陣列", "Array", "-", "-"],
    ["5", "lineNo", "WMS項次", "String(5)", "5", "V"],
    ["6", "externLineNo", "PO項次", "String(20)", "20", "V"],
    ["7", "sku", "貨號", "String(50)", "50", "V"],
    ["8", "itemName", "品名", "String(200)", "200", "V"],
    ["9", "expiryDate", "到期日", "String(10)", "-", "-"],
    ["10", "fishingGroundCode", "漁廠代碼", "String(30)", "30", "V"]
  ],
  [600, 1200, 1700, 1200, 800, 1400, 1200, 1200, 800, 1400]
), new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (成功)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "S",\n  "message": "成功",\n  "data": {\n    "detailList": [\n      {\n        "lineNo": "00001",\n        "externLineNo": "000010",\n        "sku": "SKU001",\n        "itemName": "鮭魚切片",\n        "expiryDate": "2025/12/31",\n        "fishingGroundCode": "FG001"\n      },\n      {\n        "lineNo": "00002",\n        "externLineNo": "000020",\n        "sku": "SKU002",\n        "itemName": "鱸魚",\n        "expiryDate": "2025/12/25",\n        "fishingGroundCode": "FG002"\n      }\n    ]\n  },\n  "errors": null\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (失敗)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "F",\n  "message": "失敗-檢查的資料不存在",\n  "data": null,\n  "errors": [\n    {\n      "code": "F982",\n      "message": "採購單號不存在"\n    }\n  ]\n}`));

// R3 接口
children.push(...createSection("R3-回傳收貨資料"));
children.push(createParagraph("接口資訊", true, 24));
children.push(createTable(
  ["項目", "內容"],
  [
    ["接口名稱", "回傳收貨資料"],
    ["接口代號", "R3"],
    ["URL", "POST /wmService/v1/PO/PoReceivingItem"],
    ["通訊方式", "POST"],
    ["請求格式", "JSON"],
    ["回應格式", "JSON"],
    ["提供方", "WMS"],
    ["調用方", "外部系統 (Android App)"]
  ],
  [1500, 7860]
), new Paragraph({ text: "" }));

children.push(createParagraph("功能說明", true, 24));
children.push(createParagraph("輸入收貨資料並暫存至 WMS。同一 lineNo 可多次更新。透過 requestFnName 區分「3.2確認收貨」(新增) 與「3.3修改」(更新) 操作。"));
children.push(createParagraph("requestFnName: 3.2確認收貨 (新增) / 3.3修改 (更新)"));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("請求參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "必填"],
  [
    ["1", "requestId", "請求編號 (GUID)", "String(36)", "V"],
    ["2", "requestFnName", "請求來源", "String(30)", "V"],
    ["3", "wmsAsnNumber", "WMS預期收貨單號", "String(10)", "V"],
    ["4", "lineNo", "WMS項次", "String(5)", "V"],
    ["5", "sku", "貨號", "String(50)", "V"],
    ["6", "expiryDate", "到期日", "String(10)", "V"],
    ["7", "packQty", "件數 (整數)", "int", "V"],
    ["8", "qty", "數量 (小數，條件式必填)", "decimal", "條件"],
    ["9", "batchNumber", "生產批號 (條件式必填)", "String(50)", "條件"],
    ["10", "mfgDate", "製造日 (條件式必填)", "String(10)", "條件"]
  ],
  [600, 1200, 1300, 1400, 666]
), new Paragraph({ text: "" }));

children.push(createParagraph("請求範例 (3.2確認收貨 - 新增)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "requestFnName": "3.2確認收貨",\n  "wmsAsnNumber": "0000000001",\n  "storerCode": "69512619",\n  "externReceiptNumber": "API21052600601",\n  "lineNo": "00001",\n  "externLineNo": "000010",\n  "sku": "SKU001",\n  "expiryDate": "2025/12/31",\n  "packQty": "100"\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("請求範例 (3.3修改 - 更新)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "requestFnName": "3.3修改",\n  "wmsAsnNumber": "0000000001",\n  "storerCode": "69512619",\n  "externReceiptNumber": "API21052600601",\n  "lineNo": "00001",\n  "sku": "SKU001",\n  "expiryDate": "2025/12/31",\n  "packQty": "200"\n}`));

// R4 接口
children.push(...createSection("R4-取得收貨核對明細"));
children.push(createParagraph("接口資訊", true, 24));
children.push(createTable(
  ["項目", "內容"],
  [
    ["接口名稱", "取得收貨核對明細"],
    ["接口代號", "R4"],
    ["URL", "POST /wmService/v1/PO/PoVerifying"],
    ["通訊方式", "POST"],
    ["請求格式", "JSON"],
    ["回應格式", "JSON"],
    ["提供方", "WMS"],
    ["調用方", "外部系統 (Android App)"]
  ],
  [1500, 7860]
), new Paragraph({ text: "" }));

children.push(createParagraph("功能說明", true, 24));
children.push(createParagraph("完成所有收貨項目後，取得核對資料與儲位選單。"));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "必填"],
  [
    ["1", "requestId", "請求編號", "String(36)", "V"],
    ["2", "status", "API回應結果代碼", "String(1)", "V"],
    ["3", "poTotalQty", "PO總需求公斤數", "decimal", "V"],
    ["4", "recTotalPackQty", "已收總件數", "int", "V"],
    ["5", "verifyList", "核對明細陣列", "Array", "-"],
    ["6", "locList", "儲位選單陣列", "Array", "-"]
  ],
  [1000, 1500, 666]
), new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (成功)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "S",\n  "message": "成功",\n  "data": {\n    "poTotalQty": "500.000",\n    "recTotalPackQty": "300",\n    "verifyList": [...],\n    "locList": [\n      {"locCode": "A01"},\n      {"locCode": "A02"}\n    ]\n  },\n  "errors": null\n}`));

// R5 接口
children.push(...createSection("R5-回傳全收確認"));
children.push(createParagraph("接口資訊", true, 24));
children.push(createTable(
  ["項目", "內容"],
  [
    ["接口名稱", "回傳全收確認"],
    ["接口代號", "R5"],
    ["URL", "POST /wmService/v1/PO/PoCfmReceipt"],
    ["通訊方式", "POST"],
    ["請求格式", "JSON"],
    ["回應格式", "JSON"],
    ["提供方", "WMS"],
    ["調用方", "外部系統 (Android App)"]
  ],
  [1500, 7860]
), new Paragraph({ text: "" }));

children.push(createParagraph("功能說明", true, 24));
children.push(createParagraph("輸入板數與儲位，確認過帳並產生板標籤。此接口會檢查所有 PO 項次是否皆有件數。"));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("請求參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "必填"],
  [
    ["1", "requestId", "請求編號 (GUID)", "String(36)", "V"],
    ["2", "wmsAsnNumber", "WMS預期收貨單號", "String(10)", "V"],
    ["3", "recPalletQty", "總板數 (整數)", "int", "-"],
    ["4", "recLocCode", "儲位編號", "String(10)", "V"]
  ],
  [1200, 666]
), new Paragraph({ text: "" }));

children.push(createParagraph("請求範例", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "wmsAsnNumber": "0000000001",\n  "storerCode": "69512619",\n  "externReceiptNumber": "API21052600601",\n  "recPalletQty": "5",\n  "recLocCode": "A01"\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應參數", true, 24));
children.push(createTable(
  ["項次", "參數名稱", "參數說明", "傳輸型態", "必填"],
  [
    ["1", "requestId", "請求編號", "String(36)", "V"],
    ["2", "status", "API回應結果代碼", "String(1)", "V"],
    ["3", "recPalletQty", "總板數", "int", "V"],
    ["4", "asnTotalQty", "單據總數量", "decimal", "V"],
    ["5", "recTotalPackQty", "已收總件數", "int", "V"],
    ["6", "palletLabelList", "板標籤清單", "Array", "-"]
  ],
  [1100, 666]
), new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (成功)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "S",\n  "message": "收貨確認成功",\n  "data": {\n    "recPalletQty": "5",\n    "asnTotalQty": "500.000",\n    "recTotalPackQty": "300",\n    "palletLabelList": [\n      {\n        "lblExternReceiptNumber": "採購單號 API21052600601",\n        "lblVendorName": "供應商名稱",\n        "lblPalletQty": "5",\n        "lblLocCode": "A01"\n      }\n    ]\n  },\n  "errors": null\n}`));
children.push(new Paragraph({ text: "" }));

children.push(createParagraph("回應範例 (失敗 - 項次未收完)", true, 22));
children.push(createCodeBlock(`{\n  "requestId": "fb1a6bb8-21cd-490e-9f47-962cf99ec089",\n  "status": "F",\n  "message": "失敗-資料的邏輯錯誤",\n  "data": null,\n  "errors": [\n    {\n      "code": "F983",\n      "message": "項次00010未有件數"\n    }\n  ]\n}`));

children.push(...createSection("5. 錯誤代碼說明"));
children.push(createTable(
  ["錯誤代碼", "HTTP狀態", "類別", "說明", "因應方式"],
  [
    ["F119", "401", "驗證失效", "Token 驗證失效", "重新驗證取得 Token"],
    ["F009", "500", "發生未知錯誤", "JSON 解析失敗", "檢查請求格式"],
    ["F981", "400", "資料格式錯誤", "必填欄位缺失", "修正請求資料"],
    ["F982", "404", "資料不存在", "代碼不存在", "確認資料"],
    ["F983", "400", "業務邏輯錯誤", "未定義的流程", "檢查狀態邏輯"],
    ["F984", "400", "資料重複", "請求重複", "客戶端檢查重複"],
    ["F997", "500", "伺服器忙碌", "系統忙碌", "等待後重試"],
    ["F999", "500", "伺服器處理異常", "未處理的例外", "聯繫技術支援"]
  ],
  [800, 800, 1200, 2500, 2060]
), new Paragraph({ text: "" }));

const doc = new Document({
  styles: {
    default: {
      document: {
        run: { font: "Arial", size: 22 }
      }
    },
    paragraphStyles: [
      {
        id: "Heading1",
        name: "Heading 1",
        basedOn: "Normal",
        next: "Normal",
        quickFormat: true,
        run: { size: 40, bold: true, font: "Arial", color: "000000" },
        paragraph: { spacing: { before: 240, after: 120 }, outlineLevel: 0 }
      }
    ]
  },
  sections: [{
    properties: {
      page: { size: { width: 12240, height: 15840 }, margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 } }
    },
    headers: {
      default: new Header({
        children: [
          createParagraph("機場APP", false, 22),
          new Paragraph({ children: [new TextRun("API 技術規格書")] })
        ]
      })
    },
    children: children
  }]
});

Packer.toBuffer(doc).then(buffer => {
  fs.writeFileSync("docs/api/API技術規格書_v1.01.docx", buffer);
  console.log("Word 文件已生成: docs/api/API技術規格書_v1.01.docx");
});
