// Exercise the actual browser validation functions without starting the OCR server.
import fs from 'node:fs';
import vm from 'node:vm';
import assert from 'node:assert/strict';
const base = new URL('../CoreOCROnnx.WebApi/', import.meta.url);
const source = fs.readFileSync(new URL('wwwroot/js/home-index.js', base), 'utf8');
const functions = ['validateFile', 'getExtension'].map(name => {
    const start = source.indexOf(`function ${name}(`);
    assert.ok(start >= 0, `Missing ${name}`);
    const end = source.indexOf('\nfunction ', start + 1);
    return source.slice(start, end < 0 ? undefined : end);
}).join('\n');
const context = vm.createContext({});
vm.runInContext(functions, context);
for (const ext of ['png', 'jpg', 'jpeg', 'bmp', 'JPEG'])
    assert.equal(context.validateFile({ name: `sample.${ext}`, size: 100 }), '');
for (const ext of ['tif', 'tiff', 'TIFF'])
    assert.equal(context.validateFile({ name: `sample.${ext}`, size: 100 }), '仅支持PNG/JPG/JPEG/BMP图片');
assert.equal(context.validateFile({ name: 'sample.png', size: 10 * 1024 * 1024 + 1 }), '图片不能超过10MB');
const view = fs.readFileSync(new URL('Views/Home/Index.cshtml', base), 'utf8');
const accept = view.match(/id="fileInput"[^>]*accept="([^"]+)"/)[1].split(',');
assert.deepEqual(accept, ['.png', '.jpg', '.jpeg', '.bmp', 'image/png', 'image/jpeg', 'image/bmp']);
console.log('PASS: 10 Demo frontend upload checks.');
