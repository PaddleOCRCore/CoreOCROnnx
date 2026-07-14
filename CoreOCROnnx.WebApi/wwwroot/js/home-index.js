const state = {
    file: null,
    objectUrl: "",
    result: null,
    tab: "doc",
    busy: false,
    activeBlockId: null,
    previewZoom: 1,
    showOriginImage: true
};

const previewZoomStep = 0.2;
const minPreviewZoom = 0.5;
const maxPreviewZoom = 2.5;
const defaultPreviewWidth = 560;

const app = document.getElementById("app");
const fileInput = document.getElementById("fileInput");
const dropzone = document.getElementById("dropzone");
const fileList = document.getElementById("fileList");
const sourceName = document.getElementById("sourceName");
const sourceSize = document.getElementById("sourceSize");
const modelSelect = document.getElementById("modelSelect");
const docView = document.getElementById("docView");
const resultBody = docView.parentElement;
const jsonView = document.getElementById("jsonView");
const docTab = document.getElementById("docTab");
const jsonTab = document.getElementById("jsonTab");
const copyButton = document.getElementById("copyButton");
const originToggleButton = document.getElementById("originToggleButton");
const refreshButton = document.getElementById("refreshButton");
const newButton = document.getElementById("newButton");
const licenseCodeButton = document.getElementById("licenseCodeButton");
const licenseStatusButton = document.getElementById("licenseStatusButton");
const uploadLicenseButton = document.getElementById("uploadLicenseButton");
const licenseInput = document.getElementById("licenseInput");
const statusPill = document.getElementById("statusPill");
const toast = document.getElementById("toast");
const placeholder = document.getElementById("placeholder");
const imageWrap = document.getElementById("imageWrap");
const previewImage = document.getElementById("previewImage");
const overlay = document.getElementById("overlay");
const zoomOutButton = document.getElementById("zoomOutButton");
const zoomInButton = document.getElementById("zoomInButton");
const zoomResetButton = document.getElementById("zoomResetButton");
const placeholderMark = placeholder.querySelector(".placeholder-mark");
const initialSourceName = sourceName.textContent;
const initialFileListHtml = fileList.innerHTML;
const initialDocText = docView.textContent;
const initialStatusText = statusPill.textContent;
const initialPlaceholderMark = placeholderMark ? placeholderMark.textContent : "";

fileInput.addEventListener("change", () => {
    const file = fileInput.files && fileInput.files[0];
    if (file) {
        setFile(file);
    }
});

dropzone.addEventListener("dragover", event => {
    event.preventDefault();
    dropzone.classList.add("dragging");
});

dropzone.addEventListener("dragleave", () => dropzone.classList.remove("dragging"));

dropzone.addEventListener("drop", event => {
    event.preventDefault();
    dropzone.classList.remove("dragging");
    const file = event.dataTransfer.files && event.dataTransfer.files[0];
    if (file) {
        setFile(file);
    }
});

modelSelect.addEventListener("change", () => {
    if (state.file) {
        analyze();
    }
});

refreshButton.addEventListener("click", () => analyze());
newButton.addEventListener("click", () => startNewAnalysis());
licenseCodeButton.addEventListener("click", () => getLicenseRequestCode());
licenseStatusButton.addEventListener("click", () => getLicenseStatus());
uploadLicenseButton.addEventListener("click", () => {
    if (!state.busy) {
        licenseInput.click();
    }
});
licenseInput.addEventListener("change", () => {
    const file = licenseInput.files && licenseInput.files[0];
    if (file) {
        uploadLicense(file);
    }
});
zoomOutButton.addEventListener("click", () => setPreviewZoom(state.previewZoom - previewZoomStep));
zoomInButton.addEventListener("click", () => setPreviewZoom(state.previewZoom + previewZoomStep));
zoomResetButton.addEventListener("click", () => resetPreviewZoom());
docTab.addEventListener("click", () => setTab("doc"));
jsonTab.addEventListener("click", () => setTab("json"));
if (originToggleButton) {
    originToggleButton.addEventListener("click", () => toggleOriginImage());
}

copyButton.addEventListener("click", async () => {
    const text = state.tab === "json" ? jsonView.textContent : docView.textContent;
    if (!text || text === initialDocText) {
        showToast("暂无可复制内容");
        return;
    }

    await navigator.clipboard.writeText(text);
    showToast("已复制");
});

previewImage.addEventListener("load", () => {
    applyPreviewZoom();
    renderBoxes();
});
window.addEventListener("resize", () => {
    applyPreviewZoom();
    renderBoxes();
    updateCoordinateCanvasScale();
});

function setFile(file) {
    const validation = validateFile(file);
    if (validation) {
        showToast(validation);
        return;
    }

    state.file = file;
    state.result = null;
    resetPreviewZoom(false);
    dropzone.hidden = true;
    updateFileSummary(file);
    renderLocalPreview(file);
    setResultText("解析中...");
    analyze();
}

function startNewAnalysis() {
    if (state.busy) {
        return;
    }

    state.file = null;
    state.result = null;
    resetPreviewZoom(false);
    fileInput.value = "";
    dropzone.hidden = false;
    dropzone.classList.remove("dragging");
    overlay.innerHTML = "";
    placeholder.hidden = false;
    if (placeholderMark) {
        placeholderMark.textContent = initialPlaceholderMark;
    }
    imageWrap.hidden = true;
    previewImage.removeAttribute("src");
    sourceName.textContent = initialSourceName;
    sourceSize.textContent = "";
    fileList.innerHTML = initialFileListHtml;
    setResultText(initialDocText);
    jsonView.textContent = "{}";
    setTab("doc");
    setStatus(initialStatusText);

    if (state.objectUrl) {
        URL.revokeObjectURL(state.objectUrl);
        state.objectUrl = "";
    }
}

async function analyze() {
    if (!state.file || state.busy) {
        return;
    }

    setBusy(true, "解析中...");
    setResultText("解析中...");
    overlay.innerHTML = "";

    const form = new FormData();
    form.append("file", state.file);
    form.append("model", modelSelect.value);

    try {
        const payload = await fetchJson("/OCRDemo/Analyze", {
            method: "POST",
            body: form
        });

        state.result = payload.data;
        applyResult(state.result);
        setStatus("解析完成");
    } catch (error) {
        state.result = null;
        setResultText(error.message || "解析失败");
        jsonView.textContent = "{}";
        setStatus("解析失败");
        showToast(error.message || "解析失败");
    } finally {
        setBusy(false);
    }
}

async function getLicenseRequestCode() {
    if (state.busy) {
        return;
    }

    setBusy(true, "获取授权申请码中...");
    try {
        const payload = await fetchJson("/Home/GetLicenseRequestCode");
        const requestCode = payload.data && payload.data.requestCode;
        if (!requestCode) {
            throw new Error("未获取到GPU授权申请码。");
        }

        setResultText(requestCode);
        jsonView.textContent = formatJson(payload.data);
        setStatus("授权申请码已生成");
        try {
            await navigator.clipboard.writeText(requestCode);
            showToast("授权申请码已复制");
        } catch {
            showToast("授权申请码已生成");
        }
    } catch (error) {
        setResultText(error.message || "获取授权申请码失败");
        setStatus("获取授权申请码失败");
        showToast(error.message || "获取授权申请码失败");
    } finally {
        setBusy(false);
    }
}

async function getLicenseStatus() {
    if (state.busy) {
        return;
    }

    setBusy(true, "查看授权状态中...");
    try {
        const payload = await fetchJson("/Home/GetLicenseStatus");
        const data = payload.data || {};
        setResultText(data.statusText || "未获取到授权状态。");
        jsonView.textContent = formatJson(data.modules || data);
        setStatus("授权状态已更新");
        showToast("授权状态已更新");
    } catch (error) {
        setResultText(error.message || "获取授权状态失败");
        setStatus("获取授权状态失败");
        showToast(error.message || "获取授权状态失败");
    } finally {
        setBusy(false);
    }
}

async function uploadLicense(file) {
    if (state.busy) {
        return;
    }

    const validation = validateLicenseFile(file);
    if (validation) {
        licenseInput.value = "";
        showToast(validation);
        return;
    }

    setBusy(true, "上传授权文件中...");
    const form = new FormData();
    form.append("file", file);
    try {
        const payload = await fetchJson("/Home/UploadLicense", {
            method: "POST",
            body: form
        });

        const data = payload.data || {};
        setResultText(data.statusText || "授权文件已保存到Models目录。");
        jsonView.textContent = formatJson(data);
        setStatus("授权文件有效");
        showToast("授权文件已保存");
    } catch (error) {
        setResultText(error.message || "授权文件无效");
        setStatus("授权文件无效");
        showToast(error.message || "授权文件无效");
    } finally {
        licenseInput.value = "";
        setBusy(false);
    }
}

async function fetchJson(url, options) {
    const response = await fetch(url, options);
    const payload = await response.json();
    if (String(payload.status) !== "200") {
        throw new Error(payload.errorMessage || "请求失败");
    }

    return payload;
}

function applyResult(result) {
    const content = result.markdown || result.content || "未识别到内容";
    if (!renderCoordinateResult(result)) {
        setResultText(content);
    }
    jsonView.textContent = formatJson(parseJsonValue(result.jsonText || result.raw || result));
    renderBoxes();
}

function renderLocalPreview(file) {
    overlay.innerHTML = "";
    resetPreviewZoom(false);

    if (state.objectUrl) {
        URL.revokeObjectURL(state.objectUrl);
        state.objectUrl = "";
    }

    state.objectUrl = URL.createObjectURL(file);
    previewImage.src = state.objectUrl;
    placeholder.hidden = true;
    imageWrap.hidden = false;
}

function renderBoxes() {
    overlay.innerHTML = "";
    if (!state.result || !Array.isArray(state.result.boxes) || !previewImage.naturalWidth || !previewImage.naturalHeight) {
        return;
    }

    const displayWidth = previewImage.clientWidth;
    const displayHeight = previewImage.clientHeight;
    if (!displayWidth || !displayHeight) {
        return;
    }

    const sourceWidth = state.result.imageWidth || previewImage.naturalWidth;
    const sourceHeight = state.result.imageHeight || previewImage.naturalHeight;
    const scaleX = displayWidth / sourceWidth;
    const scaleY = displayHeight / sourceHeight;
    const isYolo = state.result.model === "yolo";

    for (let index = 0; index < state.result.boxes.length; index++) {
        const item = state.result.boxes[index];
        if (!item || item.width <= 0 || item.height <= 0) {
            continue;
        }

        const box = document.createElement("div");
        box.className = `box${isYolo ? " yolo" : ""}`;
        const blockId = getBlockId(item, index);
        box.dataset.blockId = blockId;
        box.style.left = `${item.x * scaleX}px`;
        box.style.top = `${item.y * scaleY}px`;
        box.style.width = `${item.width * scaleX}px`;
        box.style.height = `${item.height * scaleY}px`;

        const label = document.createElement("div");
        label.className = "box-label";
        label.textContent = buildBoxLabel(item, index);
        box.appendChild(label);
        box.addEventListener("mouseenter", () => setActiveBlock(blockId));
        box.addEventListener("mouseleave", () => setActiveBlock(null));
        overlay.appendChild(box);
    }
    updateActiveBlock();
}

function buildBoxLabel(item, index) {
    const label = item.label || item.text || "block";
    const score = Number(item.score);
    if (Number.isFinite(score) && score > 0) {
        return `${label} ${(score * 100).toFixed(1)}%`;
    }

    return `${label} #${index + 1}`;
}

function setPreviewZoom(zoom, resetScroll = false) {
    state.previewZoom = Math.min(maxPreviewZoom, Math.max(minPreviewZoom, Number(zoom) || 1));
    applyPreviewZoom(resetScroll);
    renderBoxes();
}

function resetPreviewZoom(resetScroll = true) {
    state.previewZoom = 1;
    applyPreviewZoom(resetScroll);
    renderBoxes();
}

function applyPreviewZoom(resetScroll = false) {
    const baseWidth = getPreviewBaseWidth();
    imageWrap.style.width = `${Math.round(baseWidth * state.previewZoom)}px`;
    if (resetScroll) {
        const previewScroller = imageWrap.closest(".image-shell");
        if (previewScroller) {
            previewScroller.scrollTo({ left: 0, top: 0, behavior: "smooth" });
        }
    }
}

function getPreviewBaseWidth() {
    const shellStyle = window.getComputedStyle(imageWrap.parentElement);
    const horizontalPadding = Number.parseFloat(shellStyle.paddingLeft) + Number.parseFloat(shellStyle.paddingRight);
    const availableWidth = Math.max(240, imageWrap.parentElement.clientWidth - horizontalPadding);
    return Math.min(defaultPreviewWidth, availableWidth);
}

function renderCoordinateResult(result) {
    if (!result || result.model !== "pp-ocrv6") {
        return false;
    }

    const imageWidth = Number(result.imageWidth);
    const imageHeight = Number(result.imageHeight);
    const boxes = Array.isArray(result.boxes) ? result.boxes : [];
    const renderableBoxes = boxes
        .map((box, index) => createCoordinateGeometry(box, index))
        .filter(Boolean);
    if (!(imageWidth > 0) || !(imageHeight > 0) || !renderableBoxes.length) {
        return false;
    }

    docView.classList.remove("plain-text");
    docView.classList.add("coordinate-view");

    const stage = document.createElement("div");
    stage.className = "coordinate-stage";
    stage.dataset.imageWidth = String(imageWidth);
    stage.dataset.imageHeight = String(imageHeight);

    const canvas = document.createElement("div");
    canvas.className = "coordinate-canvas";
    canvas.style.width = `${imageWidth}px`;
    canvas.style.height = `${imageHeight}px`;
    canvas.setAttribute("aria-label", `OCR 文字复刻画布，${imageWidth} x ${imageHeight} 像素`);

    if (state.objectUrl) {
        const originImage = document.createElement("img");
        originImage.className = "origin-image";
        originImage.src = state.objectUrl;
        originImage.alt = "";
        originImage.setAttribute("aria-hidden", "true");
        canvas.appendChild(originImage);
    }

    for (const geometry of renderableBoxes) {
        canvas.appendChild(createCoordinateText(geometry));
    }

    stage.appendChild(canvas);
    docView.replaceChildren(stage);
    updateOriginImageToggle(true);
    updateCoordinateCanvasScale();
    requestAnimationFrame(() => updateCoordinateCanvasScale());
    return true;
}

function createCoordinateGeometry(box, index) {
    if (!box || !getBlockText(box).trim()) {
        return null;
    }

    const points = getCoordinatePoints(box.points || box.Points);
    if (points.length >= 4) {
        const topEdge = Math.hypot(points[1].x - points[0].x, points[1].y - points[0].y);
        const sideEdge = Math.hypot(points[2].x - points[1].x, points[2].y - points[1].y);
        const isVerticalWriting = shouldUseVerticalWriting(box, topEdge, sideEdge);
        const isVertical = !isVerticalWriting && shouldRenderVerticalText(box, topEdge, sideEdge);
        const width = isVertical ? sideEdge : topEdge;
        const height = isVertical ? topEdge : sideEdge;
        if (width > 0 && height > 0) {
            const origin = isVertical ? points[1] : points[0];
            const direction = isVertical
                ? { x: points[2].x - points[1].x, y: points[2].y - points[1].y }
                : { x: points[1].x - points[0].x, y: points[1].y - points[0].y };
            return {
                source: box,
                index,
                x: origin.x,
                y: origin.y,
                width,
                height,
                angle: Math.atan2(direction.y, direction.x) * 180 / Math.PI,
                isVertical,
                isVerticalWriting
            };
        }
    }

    const x = Number(box.x ?? box.X);
    const y = Number(box.y ?? box.Y);
    const width = Number(box.width ?? box.Width);
    const height = Number(box.height ?? box.Height);
    if (![x, y, width, height].every(Number.isFinite) || width <= 0 || height <= 0) {
        return null;
    }

    const isVerticalWriting = shouldUseVerticalWriting(box, width, height);
    const isVertical = !isVerticalWriting && shouldRenderVerticalText(box, width, height);
    return isVertical
        ? { source: box, index, x: x + width, y, width: height, height: width, angle: 90, isVertical, isVerticalWriting }
        : { source: box, index, x, y, width, height, angle: 0, isVertical, isVerticalWriting };
}

function shouldUseVerticalWriting(box, width, height) {
    if (getBlockLabel(box).toLowerCase() === "vertical_text") {
        return true;
    }

    const isTextLine = (box.isTextLine ?? box.IsTextLine) === true;
    return isTextLine && height > width * 1.5;
}

function shouldRenderVerticalText(box, width, height) {
    if (!(height > width * 1.5)) {
        return false;
    }

    const isTextLine = (box.isTextLine ?? box.IsTextLine) === true;
    return isTextLine || getBlockLabel(box).toLowerCase() === "image";
}

function getCoordinatePoints(points) {
    if (!Array.isArray(points)) {
        return [];
    }

    return points
        .map(point => ({
            x: Number(point && (point.x ?? point.X)),
            y: Number(point && (point.y ?? point.Y))
        }))
        .filter(point => Number.isFinite(point.x) && Number.isFinite(point.y))
        .slice(0, 4);
}

function createCoordinateText(geometry) {
    const box = geometry.source;
    const text = getBlockText(box);
    const blockId = getBlockId(box, geometry.index);
    const isTextLine = (box.isTextLine ?? box.IsTextLine) === true;
    const useVerticalWriting = geometry.isVerticalWriting === true;
    const useSingleLineLayout = !useVerticalWriting && (isTextLine || geometry.isVertical);
    const wrapper = document.createElement("div");
    wrapper.className = `coordinate-text${useSingleLineLayout ? "" : " coordinate-text-block"}${useVerticalWriting ? " coordinate-text-vertical" : ""}`;
    wrapper.dataset.blockId = blockId;
    wrapper.style.left = `${geometry.x}px`;
    wrapper.style.top = `${geometry.y}px`;
    wrapper.style.width = `${geometry.width}px`;
    wrapper.style.height = `${geometry.height}px`;
    wrapper.style.transform = `rotate(${geometry.angle}deg)`;
    wrapper.title = `${getBlockLabel(box)} #${blockId}`;

    const content = document.createElement("span");
    content.className = "coordinate-text-content";
    content.textContent = text;
    if (useVerticalWriting) {
        content.style.fontSize = `${getCoordinateVerticalFontSize(geometry, text)}px`;
        content.style.lineHeight = "1.15";
    } else if (useSingleLineLayout) {
        content.style.fontSize = `${Math.max(1, geometry.height * 0.9)}px`;
        content.style.lineHeight = `${geometry.height}px`;
    } else {
        content.style.fontSize = `${getCoordinateBlockFontSize(geometry, text)}px`;
        content.style.lineHeight = /\r?\n/.test(String(text || "")) ? "1.35" : "1.2";
    }

    wrapper.appendChild(content);
    wrapper.addEventListener("mouseenter", () => setActiveBlock(blockId, true));
    wrapper.addEventListener("mouseleave", () => setActiveBlock(null));

    if (useSingleLineLayout) {
        requestAnimationFrame(() => {
            if (!content.isConnected) {
                return;
            }
            const contentWidth = content.scrollWidth;
            if (contentWidth > geometry.width) {
                content.style.transform = `scaleX(${geometry.width / contentWidth})`;
            }
        });
    } else {
        requestAnimationFrame(() => fitCoordinateBlockText(content, geometry));
    }

    return wrapper;
}

function getCoordinateVerticalFontSize(geometry, text) {
    const lines = String(text || "")
        .split(/\r?\n/)
        .map(line => Array.from(line.replace(/\s/g, "")))
        .filter(line => line.length > 0);
    const characterCount = Math.max(1, lines.reduce((total, line) => total + line.length, 0));
    const columnCount = lines.length > 1
        ? lines.length
        : Math.max(1, Math.round(Math.sqrt(characterCount * geometry.width / geometry.height)));
    const charactersPerColumn = lines.length > 1
        ? Math.max(...lines.map(line => line.length))
        : Math.ceil(characterCount / columnCount);
    const fontByHeight = geometry.height / charactersPerColumn;
    const fontByWidth = geometry.width / (columnCount * 1.15);
    return Math.max(8, Math.min(fontByHeight, fontByWidth) * 0.9);
}

function getCoordinateBlockFontSize(geometry, text) {
    const lineCount = String(text || "").split(/\r?\n/).length;
    if (lineCount > 1) {
        return Math.min(64, geometry.height / (lineCount * 1.2));
    }

    return Math.min(64, Math.max(12, geometry.height * 0.75));
}

function fitCoordinateBlockText(content, geometry) {
    if (!content.isConnected) {
        return;
    }

    let fontSize = Number.parseFloat(content.style.fontSize);
    while (fontSize > 8
        && (content.scrollWidth > geometry.width + 0.5 || content.scrollHeight > geometry.height + 0.5)) {
        fontSize -= 0.5;
        content.style.fontSize = `${fontSize}px`;
    }
}

function updateCoordinateCanvasScale() {
    const stage = docView.querySelector(".coordinate-stage");
    const canvas = stage && stage.querySelector(".coordinate-canvas");
    if (!stage || !canvas || resultBody.classList.contains("show-json")) {
        return;
    }

    const imageWidth = Number(stage.dataset.imageWidth);
    const imageHeight = Number(stage.dataset.imageHeight);
    const stageStyle = window.getComputedStyle(stage);
    const horizontalBorderWidth = Number.parseFloat(stageStyle.borderLeftWidth)
        + Number.parseFloat(stageStyle.borderRightWidth);
    const verticalBorderWidth = Number.parseFloat(stageStyle.borderTopWidth)
        + Number.parseFloat(stageStyle.borderBottomWidth);
    if (!(imageWidth > 0) || !(imageHeight > 0)) {
        return;
    }

    const resultAvailableWidth = Math.max(240, docView.clientWidth - 36);
    const previewWidth = Math.min(getPreviewBaseWidth(), resultAvailableWidth);
    const scale = Math.max(0.05, (previewWidth - horizontalBorderWidth) / imageWidth);
    stage.style.width = `${previewWidth}px`;
    stage.style.height = `${imageHeight * scale + verticalBorderWidth}px`;
    canvas.style.transform = `scale(${scale})`;
}

function getBlockId(item, fallbackIndex) {
    const value = item && (item.blockId ?? item.BlockId ?? item.block_id ?? item.id);
    return value !== undefined && value !== null ? String(value) : String(fallbackIndex);
}

function getBlockLabel(item) {
    return (item && (item.label ?? item.Label ?? item.block_label)) || "block";
}

function getBlockText(item) {
    return (item && (item.text ?? item.Text ?? item.block_content)) || "";
}

function setActiveBlock(blockId, scrollPreview = false) {
    state.activeBlockId = blockId == null ? null : String(blockId);
    updateActiveBlock();

    if (scrollPreview && state.activeBlockId) {
        const activeBox = overlay.querySelector(`.box[data-block-id="${cssEscape(state.activeBlockId)}"]`);
        if (activeBox) {
            activeBox.scrollIntoView({ block: "nearest", inline: "nearest" });
        }
    }
}

function updateActiveBlock() {
    const activeId = state.activeBlockId;
    for (const box of overlay.querySelectorAll(".box")) {
        box.classList.toggle("active", !!activeId && box.dataset.blockId === activeId);
    }
    for (const block of docView.querySelectorAll(".coordinate-text")) {
        block.classList.toggle("active", !!activeId && block.dataset.blockId === activeId);
    }
}

function cssEscape(value) {
    if (window.CSS && typeof window.CSS.escape === "function") {
        return window.CSS.escape(value);
    }
    return String(value).replace(/["\\]/g, "\\$&");
}

function updateFileSummary(file) {
    sourceName.textContent = file.name;
    sourceSize.textContent = formatFileSize(file.size);
    fileList.innerHTML = "";

    const item = document.createElement("div");
    item.className = "file-item";
    item.innerHTML = `<div class="file-icon">图</div><div><div class="file-name"></div><div class="file-meta"></div></div>`;
    item.querySelector(".file-name").textContent = file.name;
    item.querySelector(".file-meta").textContent = `${formatFileSize(file.size)} · ${modelSelect.options[modelSelect.selectedIndex].text}`;
    fileList.appendChild(item);
}

function setTab(tab) {
    state.tab = tab;
    docTab.classList.toggle("active", tab === "doc");
    jsonTab.classList.toggle("active", tab === "json");
    resultBody.classList.toggle("show-json", tab === "json");
}

function setResultText(text) {
    state.activeBlockId = null;
    docView.classList.add("plain-text");
    docView.classList.remove("coordinate-view");
    docView.textContent = text || "";
    updateOriginImageToggle(false);
}

function toggleOriginImage() {
    state.showOriginImage = !state.showOriginImage;
    updateOriginImageToggle(!!docView.querySelector(".coordinate-stage"));
}

function updateOriginImageToggle(available) {
    if (!originToggleButton) {
        return;
    }

    originToggleButton.hidden = !available;
    originToggleButton.classList.toggle("active", available && state.showOriginImage);
    originToggleButton.title = state.showOriginImage ? "隐藏底图" : "显示底图";
    originToggleButton.setAttribute("aria-label", originToggleButton.title);
    docView.classList.toggle("origin-hidden", !state.showOriginImage);
}

function setStatus(text) {
    statusPill.textContent = text;
}

function setBusy(busy, statusText) {
    state.busy = busy;
    app.classList.toggle("busy", busy);
    for (const element of [fileInput, modelSelect, refreshButton, newButton, licenseCodeButton, licenseStatusButton, uploadLicenseButton]) {
        element.disabled = busy;
    }
    if (statusText) {
        setStatus(statusText);
    }
}

function validateFile(file) {
    const extension = getExtension(file.name);
    if (![".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff"].includes(extension)) {
        return "仅支持PNG/JPG/BMP/TIF图片";
    }

    if (file.size > 10 * 1024 * 1024) {
        return "图片不能超过10MB";
    }

    return "";
}

function validateLicenseFile(file) {
    if (getExtension(file.name) !== ".lic") {
        return "请上传.lic授权文件";
    }

    if (file.size > 1024 * 1024) {
        return "授权文件不能超过1MB";
    }

    return "";
}

function getExtension(fileName) {
    const index = fileName.lastIndexOf(".");
    return index >= 0 ? fileName.slice(index).toLowerCase() : "";
}

function formatFileSize(size) {
    if (!Number.isFinite(size)) {
        return "";
    }

    if (size < 1024) {
        return `${size} B`;
    }

    if (size < 1024 * 1024) {
        return `${(size / 1024).toFixed(1)} KB`;
    }

    return `${(size / 1024 / 1024).toFixed(2)} MB`;
}

function parseJsonValue(value) {
    if (typeof value !== "string") {
        return value;
    }

    const trimmed = value.trim();
    if (!trimmed || !/^[\[{]/.test(trimmed)) {
        return value;
    }

    try {
        return JSON.parse(trimmed);
    } catch {
        return value;
    }
}

function formatJson(value) {
    if (typeof value === "string") {
        const parsed = parseJsonValue(value);
        if (parsed !== value) {
            return JSON.stringify(parsed, null, 2);
        }

        return value;
    }

    return JSON.stringify(value || {}, null, 2);
}

function showToast(message) {
    toast.textContent = message;
    toast.classList.add("show");
    clearTimeout(showToast.timer);
    showToast.timer = setTimeout(() => toast.classList.remove("show"), 2200);
}