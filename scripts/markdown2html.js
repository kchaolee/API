#!/usr/bin/env node
// Minimal Markdown to HTML converter for docs/api
// Converts a fixed set of Markdown files under docs/api/ into docs/api/html/
// and produces an index.html listing all output pages.

const fs = require('fs');
const path = require('path');
let md = null;
try {
  md = require('markdown-it')({ html: true, linkify: true, typographer: true });
} catch (e) {
  console.error('Missing markdown-it. Run: npm i markdown-it');
  process.exit(1);
}

const projectRoot = path.resolve(__dirname, '..');
const srcRoot = path.join(projectRoot, 'docs', 'api');
const outRoot = path.join(srcRoot, 'html');

const targets = [
  'INDEX.md',
  'README.md',
  path.join('authentication', 'A1-登入驗證.md'),
  path.join('receiving', 'R1-取得預期收貨清單.md'),
  path.join('receiving', 'R2-取得收貨明細資料.md'),
  path.join('receiving', 'R3-回傳收貨資料.md'),
  path.join('receiving', 'R4-取得收貨核對明細.md'),
  path.join('receiving', 'R5-回傳全收確認.md'),
];

function ensureDir(p) {
  if (!fs.existsSync(p)) {
    fs.mkdirSync(p, { recursive: true });
  }
}

function toHtmlFile(mdPath) {
  const rel = path.relative(srcRoot, mdPath);
  const htmlPath = path.join(outRoot, rel).replace(/\\/g, '/').replace(/\.md$/i, '.html');
  const dir = path.dirname(htmlPath);
  ensureDir(dir);
  return { htmlPath, relPath: rel.replace(/\\/g, '/').replace(/\.md$/i, '.html') };
}

function buildTemplate(contentHtml, title) {
  const tpl = `<!doctype html>
<html lang="zh-Hant">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>${title}</title>
  <link rel="stylesheet" href="styles.css" />
  <style> :root{ --bg: #f6f7fb; --card:#fff; } </style>
  </head>
<body>
  <div class="site-wrap">
    <header class="site-header">
      <h1 class="site-title">API 技術規格 - 靜態檔</h1>
    </header>
    <main class="content">
${contentHtml}
    </main>
  </div>
  <script>/* no scripts by default */</script>
  </body>
  </html>`;
  return tpl;
}

function writeHtmlFile(htmlPath, htmlContent) {
  fs.writeFileSync(htmlPath, htmlContent, 'utf8');
}

function generateIndex(allPages) {
  const links = allPages.map(p => {
    const url = (p.path || '').replace(/\\/g, '/');
    const title = p.title || path.basename(p.path, '.html');
    return `<li><a href="${url}">${title}</a></li>`;
  }).join('\n');
  const indexHtml = `<!doctype html>
<html lang="zh-Hant">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>API Markdown HTML 導覽</title>
  <link rel="stylesheet" href="styles.css" />
</head>
<body>
  <div class="site-wrap">
    <header class="site-header"><h1 class="site-title">Docs API - 靜态 HTML 導覽</h1></header>
    <nav class="toc">
      <ul>
        ${links}
      </ul>
    </nav>
  </div>
</body>
</html>`;
  writeHtmlFile(path.join(outRoot, 'index.html'), indexHtml);
}

function convertMdFile(srcPath) {
  const { htmlPath, relPath } = toHtmlFile(srcPath);
  const mdContent = fs.readFileSync(srcPath, 'utf8');
  const htmlContent = md.render(mdContent);
  // Simple wrapper
  const title = path.basename(srcPath, '.md');
  const fullHtml = buildTemplate(htmlContent, title);
  writeHtmlFile(htmlPath, fullHtml);
  return { path: path.relative(outRoot, htmlPath), title };
}

function main() {
  // ensure output dir
  ensureDir(outRoot);
  // process targets in docs/api
  const pages = [];
  targets.forEach(rel => {
    const abs = path.join(srcRoot, rel);
    if (fs.existsSync(abs)) {
      const info = convertMdFile(abs);
      pages.push({ path: info.path, title: info.title });
    } else {
      console.warn('Skip missing:', abs);
    }
  });
  // generate index with found pages
  const indexPages = pages.map(p => ({ path: p.path, title: p.title }));
  if (indexPages.length > 0) {
    // generate links with forward-slash URLs for consistency
    const links = indexPages.map(q => {
      const url = (q.path || '').replace(/\\/g, '/');
      return `<li><a href="${url}">${q.title}</a></li>`;
    }).join('');
    writeHtmlFile(path.join(outRoot, 'index.html'), '<!doctype html><html><head><meta charset="utf-8"><link rel="stylesheet" href="styles.css"/><title>API HTML List</title></head><body><h1>API HTML List</h1><ul>' + links + '</ul></body></html>');
  }
  console.log('HTML conversion finished. Output at', outRoot);
}

main();
