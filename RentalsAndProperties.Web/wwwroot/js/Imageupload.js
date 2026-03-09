/**
 * RentalsAndProperties – Image Upload JS (Phase 3)
 * Handles: drag-drop, preview grid, file validation, upload progress
 */
'use strict';

/**
 * Initialize the image upload widget.
 * @param {Object} opts - Configuration options
 */
function initImageUpload(opts = {}) {
    const {
        dropZoneId      = 'dropZone',
        fileInputId     = 'fileInput',
        previewGridId   = 'newPreviewGrid',
        uploadActionsId = 'uploadActions',
        uploadBtnId     = 'uploadBtn',
        clearBtnId      = 'clearBtn',
        selectedCountId = 'selectedCount',
        maxFiles        = 10,
        maxSizeMB       = 5
    } = opts;

    const dropZone    = document.getElementById(dropZoneId);
    const fileInput   = document.getElementById(fileInputId);
    const previewGrid = document.getElementById(previewGridId);
    const uploadActions = document.getElementById(uploadActionsId);
    const uploadBtn   = document.getElementById(uploadBtnId);
    const clearBtn    = document.getElementById(clearBtnId);
    const selectedCount = document.getElementById(selectedCountId);

    if (!dropZone || !fileInput) return;

    const ALLOWED_TYPES = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    const MAX_BYTES     = maxSizeMB * 1024 * 1024;
    let selectedFiles   = [];

    // ── Drop zone interactions ───────────────────────────────────────────────

    dropZone.addEventListener('click', () => fileInput.click());

    dropZone.addEventListener('keydown', e => {
        if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); fileInput.click(); }
    });

    dropZone.addEventListener('dragover', e => {
        e.preventDefault();
        dropZone.classList.add('drag-active');
    });

    dropZone.addEventListener('dragleave', e => {
        if (!dropZone.contains(e.relatedTarget))
            dropZone.classList.remove('drag-active');
    });

    dropZone.addEventListener('drop', e => {
        e.preventDefault();
        dropZone.classList.remove('drag-active');
        handleFiles(Array.from(e.dataTransfer.files));
    });

    fileInput.addEventListener('change', () => {
        handleFiles(Array.from(fileInput.files));
        fileInput.value = ''; // reset so same file can be re-picked
    });

    // ── File handling ────────────────────────────────────────────────────────

    function handleFiles(files) {
        for (const file of files) {
            if (selectedFiles.length >= maxFiles) {
                Toast?.error(`Maximum ${maxFiles} images allowed.`);
                break;
            }

            if (!ALLOWED_TYPES.includes(file.type)) {
                Toast?.error(`"${file.name}" is not a supported image type.`);
                continue;
            }

            if (file.size > MAX_BYTES) {
                Toast?.error(`"${file.name}" exceeds the ${maxSizeMB}MB size limit.`);
                continue;
            }

            // Deduplicate by name+size
            const isDuplicate = selectedFiles.some(
                f => f.name === file.name && f.size === file.size
            );
            if (isDuplicate) continue;

            selectedFiles.push(file);
        }

        rebuildPreview();
        syncFileInput();
        updateUI();
    }

    // ── Preview grid ─────────────────────────────────────────────────────────

    function rebuildPreview() {
        if (!previewGrid) return;
        previewGrid.innerHTML = '';

        selectedFiles.forEach((file, idx) => {
            const reader = new FileReader();
            reader.onload = e => {
                const item = document.createElement('div');
                item.className = 'new-preview-item';
                item.innerHTML = `
                    <img src="${e.target.result}" alt="${file.name}" loading="lazy" />
                    ${idx === 0 ? '<div class="new-preview-badge">Cover</div>' : ''}
                    <button type="button" class="new-preview-remove"
                            data-idx="${idx}" title="Remove"
                            aria-label="Remove ${file.name}">✕</button>
                `;

                item.querySelector('.new-preview-remove').addEventListener('click', () => {
                    selectedFiles.splice(idx, 1);
                    rebuildPreview();
                    syncFileInput();
                    updateUI();
                });

                previewGrid.appendChild(item);
            };
            reader.readAsDataURL(file);
        });
    }

    // ── Sync DataTransfer → file input ───────────────────────────────────────

    function syncFileInput() {
        try {
            const dt = new DataTransfer();
            selectedFiles.forEach(f => dt.items.add(f));
            fileInput.files = dt.files;
        } catch {
            // Safari fallback: DataTransfer not supported — rely on server-side handling
        }
    }

    // ── UI state ─────────────────────────────────────────────────────────────

    function updateUI() {
        const hasFiles = selectedFiles.length > 0;

        if (uploadActions)
            uploadActions.style.display = hasFiles ? 'flex' : 'none';

        if (selectedCount)
            selectedCount.textContent = hasFiles
                ? `${selectedFiles.length} image${selectedFiles.length > 1 ? 's' : ''} selected`
                : '';

        if (dropZone) {
            dropZone.querySelector('.drop-zone-sub')!.textContent =
                `${selectedFiles.length}/${maxFiles} selected · JPG, PNG, WebP · Max ${maxSizeMB}MB each`;
        }
    }

    // ── Clear button ─────────────────────────────────────────────────────────

    clearBtn?.addEventListener('click', () => {
        selectedFiles = [];
        rebuildPreview();
        syncFileInput();
        updateUI();
    });

    // ── Upload form submit with loading state ─────────────────────────────────

    uploadBtn?.closest('form')?.addEventListener('submit', e => {
        if (selectedFiles.length === 0) {
            e.preventDefault();
            Toast?.error('Please select at least one image.');
            return;
        }
        setButtonLoading?.(uploadBtn, true);
        simulateProgress();
    });

    // ── Fake progress bar (real progress needs XHR) ───────────────────────────

    function simulateProgress() {
        const bar   = document.getElementById('uploadProgressBar');
        const label = document.getElementById('uploadProgressLabel');
        const wrap  = document.getElementById('uploadProgressWrap');

        if (!bar || !wrap) return;
        wrap.style.display = 'block';

        let pct = 0;
        const interval = setInterval(() => {
            pct = Math.min(pct + Math.random() * 18, 90);
            bar.style.width = pct + '%';
            if (label) label.textContent = `Uploading… ${Math.round(pct)}%`;
        }, 200);

        // Will complete once page reloads from form submit
        window._uploadInterval = interval;
    }
}

// ── Expose globally ──────────────────────────────────────────────────────────
window.initImageUpload = initImageUpload;