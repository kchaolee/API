#!/usr/bin/env node
// Convert static HTML index/html views to Word documents for sharing inside Claude workflows.
// This is a lightweight HTML->DOCX converter with a simple mapping.

const fs = require('fs');
const path = require('path');
const { Document, Packer, Paragraph, TextRun, ExternalHyperlink, HeadingLevel } = require('docx');

function ensureDir(p){ if(!fs.existsSync(p)) fs.mkdirSync(p,{recursive:true}); }

const htmlRoot = path.resolve(__dirname, '..', 'docs', 'api', 'html');
const docxOutRoot = path.join(htmlRoot, 'docx');
ensureDir(docxOutRoot);

function toDocxPath(htmlPath){
  const rel = path.relative(htmlRoot, htmlPath);
  const withoutExt = rel.replace(/\\/g, '/').replace(/\.html$/i, '');
  // create directory structure inside docx
  const outDir = path.join(docxOutRoot, path.dirname(rel));
  ensureDir(outDir);
  return path.join(outDir, path.basename(rel, '.html') + '.docx');
}

function htmlToText(html){
  // very naive text extraction
  let t = html
    .replace(/<[^>]+>/g, ' ')
    .replace(/&nbsp;/g, ' ')
    .replace(/&lt;/g, '<').replace(/&gt;/g, '>')
    .replace(/&amp;/g, '&');
  return t.replace(/\s+/g, ' ').trim();
}

function parseHtmlToDocs(htmlContent){
  // naive tag-based parse: h1/h2/h3 -> headings, p -> text, a -> hyperlink, ul/li as bullets, pre/code as code blocks
  const blocks = [];
  const tagRE = /<\s*(h[1-6]|p|pre|code|a|ul|li)\b[^>]*>([\s\S]*?)<\/\1>/g;
  let m;
  let inUl = false;
  while((m = tagRE.exec(htmlContent)) !== null){
    const tag = m[1].toLowerCase();
    const inner = m[2];
    if(tag.startsWith('h')){
      const level = parseInt(tag.substring(1), 10);
      const text = htmlToText(inner);
      const p = new Paragraph({ text: text, heading: level <= 5 ? HeadingLevel[`HEADING_${level}`] : HeadingLevel.HEADING_2 });
      blocks.push(p);
    } else if(tag === 'p'){
      const text = htmlToText(inner);
      blocks.push(new Paragraph({ children: [new TextRun(text)] }));
    } else if(tag === 'a'){
      // extract href and text
      const href = (m[0].match(/href=\"([^\"]+)\"/)||[])[1] || '#';
      const text = htmlToText(inner);
      blocks.push(new Paragraph({ children: [new ExternalHyperlink({ link: href, children: [new TextRun(text)] })] }));
    } else if(tag === 'li'){
      const text = htmlToText(inner);
      blocks.push(new Paragraph({ children: [new TextRun({ text: text, bold: false })], numbering: { // simple bullet as text
        }, }));
      // Fallback approach: prefix bullet manually
      blocks[blocks.length-1] = new Paragraph({ children: [new TextRun("• " + text)] });
    } else if(tag === 'ul'){
      inUl = true;
    } else if(tag === 'pre' || tag === 'code'){
      const codeText = inner.replace(/&lt;/g,'<').replace(/&gt;/g,'>');
      blocks.push(new Paragraph({ children: [new TextRun({ text: codeText, font: 'Courier New' })] }));
    }
  }
  // If nothing parsed, fallback basic text
  if (blocks.length === 0) {
    blocks.push(new Paragraph({ children: [new TextRun(htmlContent.replace(/<[^>]+>/g, ''))] }));
  }
  return blocks;
}

function convertHtmlFile(htmlPath){
  const html = fs.readFileSync(htmlPath, 'utf8');
  const docxPath = toDocxPath(htmlPath);
  const contentBlocks = parseHtmlToDocs(html);
  const doc = new Document({ sections: [{ properties: { page: { size: { width: 12240, height: 15840 }, margin: { top: 720, right: 720, bottom: 720, left: 720 } } }, children: contentBlocks }] });
  return { doc, docxPath };
}

function walkDir(dir, cb){
  fs.readdirSync(dir, { withFileTypes: true }).forEach(d => {
    const p = path.join(dir, d.name);
    if (d.isDirectory()) walkDir(p, cb);
    else cb(p);
  });
}

async function main(){
  const htmlDir = path.resolve(htmlRoot);
  const files = [];
  walkDir(htmlDir, (p)=>{ if (p.endsWith('.html') && !p.endsWith('index.html') && !p.endsWith('INDEX.html')) files.push(p); });
  for (const htmlPath of files){
    const { doc, docxPath } = convertHtmlFile(htmlPath);
    const buf = await docxBuffer(doc);
    fs.writeFileSync(docxPath, buf);
    console.log('DOCX written:', docxPath);
  }
}

function docxBuffer(doc){
  return new Promise((resolve, reject)=>{
    Packer.toBuffer(doc).then(resolve).catch(reject);
  });
}

main().catch(e => console.error(e));
