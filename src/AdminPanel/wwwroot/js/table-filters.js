
(function () {
    'use strict';

    const DEBOUNCE_MS = 400;

    // ── Boot ────────────────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', init);

    function init() {
        document.querySelectorAll('[data-tf-toggle]').forEach(bindToggleButton);
        //document.querySelectorAll('[data-tf-panel]').forEach(bindPanel);
        document.addEventListener('keydown', onKeyDown);
    }

    // ── Toggle button ────────────────────────────────────────────────────────
    function bindToggleButton(btn) {
        const panelId = btn.dataset.tfToggle;
        const panel   = document.getElementById(panelId);
        if (!panel) return;

        // Restore saved open state
        if (sessionStorage.getItem('tf-open:' + panelId) === '1') {
            openPanel(panel, btn);
        }

        btn.addEventListener('click', () => {
            if (panel.classList.contains('tf-panel--open')) {
                closePanel(panel, btn);
                sessionStorage.removeItem('tf-open:' + panelId);
            } else {
                openPanel(panel, btn);
                sessionStorage.setItem('tf-open:' + panelId, '1');
            }
        });
    }

    function openPanel(panel, btn) {
        panel.classList.add('tf-panel--open');
        if (btn) { btn.classList.add('tf-toggle-btn--active'); btn.setAttribute('aria-expanded', 'true'); }
    }

    function closePanel(panel, btn) {
        panel.classList.remove('tf-panel--open');
        if (btn) { btn.classList.remove('tf-toggle-btn--active'); btn.setAttribute('aria-expanded', 'false'); }
    }

    // ── Panel internals ──────────────────────────────────────────────────────
    function bindPanel(panel) {
        const form = panel.closest('form');
        if (!form) return;

        // ── SELECT → auto-submit immediately ────────────────────────────
        //panel.querySelectorAll('[data-tf-select]').forEach(sel => {
        //    sel.addEventListener('change', () => { resetPage(form); form.submit(); });
        //});

        // ── TEXT / DATE → debounced auto-submit ─────────────────────────
        //panel.querySelectorAll('[data-tf-text]').forEach(input => {
        //    let timer;
        //    input.addEventListener('input', () => {
        //        clearTimeout(timer);
        //        updateClearBtn(input);
        //        timer = setTimeout(() => { resetPage(form); form.submit(); }, DEBOUNCE_MS);
        //    });
        //});

        // ── Clear × button — static ones rendered by Razor ──────────────
        //panel.querySelectorAll('[data-tf-clear-input]').forEach(btn => {
        //    btn.addEventListener('click', () => clearInputAndSubmit(btn, form));
        //});

        //// ── Clear × button — dynamic ones added by updateClearBtn ───────
        //panel.addEventListener('click', e => {
        //    const btn = e.target.closest('[data-tf-clear-input]');
        //    if (btn) clearInputAndSubmit(btn, form);
        //});

        // ── RADIO → live active-class + auto-submit ──────────────────────
        //panel.querySelectorAll('[data-tf-radio]').forEach(group => {
        //    group.querySelectorAll('input[type="radio"]').forEach(radio => {
        //        radio.addEventListener('change', () => {
        //            // Sync active class on all labels within this group
        //            group.querySelectorAll('.tf-radio-label').forEach(lbl => {
        //                const inp = lbl.querySelector('input[type="radio"]');
        //                lbl.classList.toggle('tf-radio-label--on', inp?.checked ?? false);
        //            });
        //            resetPage(form);
        //            form.submit();
        //        });
        //    });
        //});

        // ── CHECKBOX → aggregate checked values into hidden input ────────
        //panel.querySelectorAll('[data-tf-checkbox]').forEach(group => {
        //    const hidden = group.previousElementSibling; // [data-tf-checkbox-hidden]
        //    if (!hidden) return;

        //    group.querySelectorAll('input[type="checkbox"]').forEach(cb => {
        //        cb.addEventListener('change', () => {
        //            // Sync active class on this label
        //            const lbl = cb.closest('.tf-check-label');
        //            if (lbl) lbl.classList.toggle('tf-check-label--on', cb.checked);

        //            // Write comma-joined checked values to hidden field
        //            const checked = [...group.querySelectorAll('input[type="checkbox"]:checked')]
        //                .map(c => c.value)
        //                .join(',');
        //            hidden.value = checked;

        //            resetPage(form);
        //            form.submit();
        //        });
        //    });
        //});

        // ── NUMBER COMPARISON → toggle between single and range inputs ───
        //panel.querySelectorAll('[data-tf-numop]').forEach(wrap => {
        //    const opSelect   = wrap.querySelector('[data-tf-op-select]');
        //    const betweenFld = wrap.querySelector('[data-tf-between-field]');
        //    if (!opSelect) return;

        //    opSelect.addEventListener('change', () => {
        //        const isBetween = opSelect.value === 'between';
        //        if (betweenFld) betweenFld.classList.toggle('tf-numcmp-to--hidden', !isBetween);
        //        // Clear max value when switching away from "between"
        //        if (!isBetween && betweenFld) {
        //            const maxInput = betweenFld.querySelector('input[type="number"]');
        //            if (maxInput) maxInput.value = '';
        //        }
        //        resetPage(form);
        //        form.submit();
        //    });
        //});
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    function clearInputAndSubmit(btn, form) {
        const wrap  = btn.closest('.tf-ctrl-wrap');
        const input = wrap?.querySelector('[data-tf-text]');
        if (input) { input.value = ''; input.classList.remove('tf-ctrl--active'); }
        btn.remove();
        resetPage(form);
        form.submit();
    }

    /** Show/hide the per-input clear "×" button based on whether input has a value. */
    function updateClearBtn(input) {
        const wrap = input.closest('.tf-ctrl-wrap');
        if (!wrap) return;

        let x = wrap.querySelector('[data-tf-clear-input]');

        if (input.value.trim()) {
            input.classList.add('tf-ctrl--active');
            if (!x) {
                x = document.createElement('button');
                x.type = 'button';
                x.className = 'tf-x';
                x.dataset.tfClearInput = '';
                x.title = 'Clear';
                x.textContent = '×';
                wrap.appendChild(x);
            }
        } else {
            input.classList.remove('tf-ctrl--active');
            x?.remove();
        }
    }

    /** Reset pagination to page 1 before any filter submit. */
    function resetPage(form) {
        const p = form.querySelector('input[name="page"]');
        if (p) p.value = '1';
    }

    /** Escape closes any open panel. */
    function onKeyDown(e) {
        if (e.key !== 'Escape') return;
        document.querySelectorAll('.tf-panel--open').forEach(panel => {
            const btn = document.querySelector(`[data-tf-toggle="${panel.id}"]`);
            closePanel(panel, btn);
        });
    }

})();
