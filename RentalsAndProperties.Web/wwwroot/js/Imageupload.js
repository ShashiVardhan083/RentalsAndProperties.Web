/**
 * RentalsAndProperties – Image Upload JS
 * Handles: drag-drop, preview grid, file validation, upload progress
 *
 * FIXED:
 *  - Removed TypeScript "!" non-null assertion that breaks in plain JS
 *  - Added null guard on drop-zone-sub element
 */
'use strict';

/**
 * Initialize the image upload widget.
 * @param {Object} opts - Configuration options
 */
function initImageUpload(opts) {
    opts = opts || {};

    var dropZoneId = opts.dropZoneId || 'dropZone';
    var fileInputId = opts.fileInputId || 'fileInput';
    var previewGridId = opts.previewGridId || 'newPreviewGrid';
    var uploadActionsId = opts.uploadActionsId || 'uploadActions';
    var uploadBtnId = opts.uploadBtnId || 'uploadBtn';
    var clearBtnId = opts.clearBtnId || 'clearBtn';
    var selectedCountId = opts.selectedCountId || 'selectedCount';
    var maxFiles = opts.maxFiles || 10;
    var maxSizeMB = opts.maxSizeMB || 5;

    var dropZone = document.getElementById(dropZoneId);
    var fileInput = document.getElementById(fileInputId);
    var previewGrid = document.getElementById(previewGridId);
    var uploadActions = document.getElementById(uploadActionsId);
    var uploadBtn = document.getElementById(uploadBtnId);
    var clearBtn = document.getElementById(clearBtnId);
    var selectedCount = document.getElementById(selectedCountId);

    // Bail out if the required elements aren't on this page
    if (!dropZone || !fileInput) return;

    var ALLOWED_TYPES = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    var MAX_BYTES = maxSizeMB * 1024 * 1024;
    var selectedFiles = [];

    // ── Drop zone interactions ────────────────────────────────────────────────

    dropZone.addEventListener('click', function () { fileInput.click(); });

    dropZone.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            fileInput.click();
        }
    });

    dropZone.addEventListener('dragover', function (e) {
        e.preventDefault();
        dropZone.classList.add('drag-active');
    });

    dropZone.addEventListener('dragleave', function (e) {
        if (!dropZone.contains(e.relatedTarget)) {
            dropZone.classList.remove('drag-active');
        }
    });

    dropZone.addEventListener('drop', function (e) {
        e.preventDefault();
        dropZone.classList.remove('drag-active');
        handleFiles(Array.from(e.dataTransfer.files));
    });

    fileInput.addEventListener('change', function () {
        handleFiles(Array.from(fileInput.files));
        fileInput.value = ''; // reset so the same file can be re-picked
    });

    // ── File handling ─────────────────────────────────────────────────────────

    function handleFiles(files) {
        for (var i = 0; i < files.length; i++) {
            var file = files[i];

            if (selectedFiles.length >= maxFiles) {
                if (typeof Toast !== 'undefined') Toast.error('Maximum ' + maxFiles + ' images allowed.');
                break;
            }

            if (!ALLOWED_TYPES.includes(file.type)) {
                if (typeof Toast !== 'undefined') Toast.error('"' + file.name + '" is not a supported image type.');
                continue;
            }

            if (file.size > MAX_BYTES) {
                if (typeof Toast !== 'undefined') Toast.error('"' + file.name + '" exceeds the ' + maxSizeMB + 'MB size limit.');
                continue;
            }

            // Deduplicate by name + size
            var isDuplicate = selectedFiles.some(function (f) {
                return f.name === file.name && f.size === file.size;
            });
            if (isDuplicate) continue;

            selectedFiles.push(file);
        }

        rebuildPreview();
        syncFileInput();
        updateUI();
    }

    // ── Preview grid ──────────────────────────────────────────────────────────

    function rebuildPreview() {
        if (!previewGrid) return;
        previewGrid.innerHTML = '';

        selectedFiles.forEach(function (file, idx) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var item = document.createElement('div');
                item.className = 'new-preview-item';
                item.innerHTML =
                    '<img src="' + e.target.result + '" alt="' + file.name + '" loading="lazy" />' +
                    (idx === 0 ? '<div class="new-preview-badge">Cover</div>' : '') +
                    '<button type="button" class="new-preview-remove" data-idx="' + idx + '" title="Remove" aria-label="Remove ' + file.name + '">&#x2715;</button>';

                item.querySelector('.new-preview-remove').addEventListener('click', function () {
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

    // ── Sync DataTransfer → file input ────────────────────────────────────────

    function syncFileInput() {
        try {
            var dt = new DataTransfer();
            selectedFiles.forEach(function (f) { dt.items.add(f); });
            fileInput.files = dt.files;
        } catch (err) {
            // Safari fallback: DataTransfer.items not supported — the form will still
            // submit whatever is currently in the real file input.
            console.warn('DataTransfer not supported, falling back:', err);
        }
    }

    // ── UI state ──────────────────────────────────────────────────────────────

    function updateUI() {
        var hasFiles = selectedFiles.length > 0;

        if (uploadActions) {
            uploadActions.style.display = hasFiles ? 'flex' : 'none';
        }

        if (selectedCount) {
            selectedCount.textContent = hasFiles
                ? selectedFiles.length + ' image' + (selectedFiles.length > 1 ? 's' : '') + ' selected'
                : '';
        }

        // Update the sub-text inside the drop zone (null-safe)
        if (dropZone) {
            var subEl = dropZone.querySelector('.drop-zone-sub');
            if (subEl) {
                subEl.textContent =
                    selectedFiles.length + '/' + maxFiles + ' selected · JPG, PNG, WebP · Max ' + maxSizeMB + 'MB each';
            }
        }
    }

    // ── Clear button ──────────────────────────────────────────────────────────

    if (clearBtn) {
        clearBtn.addEventListener('click', function () {
            selectedFiles = [];
            rebuildPreview();
            syncFileInput();
            updateUI();
        });
    }

    // ── Upload form submit with loading state ─────────────────────────────────

    var uploadForm = uploadBtn ? uploadBtn.closest('form') : null;
    if (uploadForm) {
        uploadForm.addEventListener('submit', function (e) {
            if (selectedFiles.length === 0) {
                e.preventDefault();
                if (typeof Toast !== 'undefined') Toast.error('Please select at least one image.');
                return;
            }
            if (typeof setButtonLoading === 'function') setButtonLoading(uploadBtn, true);
            simulateProgress();
        });
    }

    // ── Simulated progress bar ────────────────────────────────────────────────

    function simulateProgress() {
        var bar = document.getElementById('uploadProgressBar');
        var label = document.getElementById('uploadProgressLabel');
        var wrap = document.getElementById('uploadProgressWrap');

        if (!bar || !wrap) return;
        wrap.style.display = 'block';

        var pct = 0;
        var interval = setInterval(function () {
            pct = Math.min(pct + Math.random() * 18, 90);
            bar.style.width = pct + '%';
            if (label) label.textContent = 'Uploading… ' + Math.round(pct) + '%';
        }, 200);

        // Store so it can be cleared if needed
        window._uploadInterval = interval;
    }
}

// Expose globally
window.initImageUpload = initImageUpload;