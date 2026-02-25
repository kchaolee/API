#!/usr/bin/env node
// Convert docs/api/html/index.html into a Word document
// The resulting .docx will mirror the HTML index structure with hyperlinks

const fs = require('fs');
const path = require('path');
let { Document, Packer, Paragraph, ExternalHyperlink, TextRun, HeadingLevel } = require('docx');

function readIndexLinks(indexPath) {
  const html = fs.readFileSync(indexPath, 'utf8');
  const anchors = [];
  const re = /<a\s+[^>]*href=\"([^\"]+)\"[^>]*>([^<]+)<\/a>/g;
  let m;
  while ((m = re.exec(html)) !== null) {
    let href = m[1];
    let text = m[2];
    if (href && href.endsWith('.html')) {
      // normalize path to use / separators
      anchors.push({ href: href.replace(/\\/g, '/'), text: text.trim() });
    }
  }
  return anchors;
}

function buildDoc(links, title) {
  const sections = [];
  sections.push(new Paragraph({ text: title, heading: HeadingLevel.HEADING_1 }));
  for (const l of links) {
    sections.push(new Paragraph({
      children: [new ExternalHyperlink({ link: l.href, children: [new TextRun(l.text)] })]
    }));
  }
  const doc = new Document({
    sections: [{
      properties: {
        page: {
          size: { width: 12240, height: 15840 },
          margin: { top: 720, right: 720, bottom: 720, left: 720 }
        }
      },
      children: sections
    }]
  });
  return doc;
}

function main() {
  const indexPath = path.resolve(__dirname, '../docs/api/html/index.html');
  const outputPath = path.resolve(__dirname, '../docs/api/html/INDEX.html.docx');
  if (!fs.existsSync(indexPath)) {
    console.error('Index file not found:', indexPath);
    process.exit(1);
  }
  // read links from index.html
  const links = readIndexLinks(indexPath);
  // derive title from first heading in the HTML or default
  const title = 'API HTML List';
  const doc = buildDoc(links, title);
  Packer.toBuffer(doc).then((buffer) => {
    fs.writeFileSync(outputPath, buffer);
    console.log('Word document generated:', outputPath);
  });
}

main();
