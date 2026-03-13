/**
 * product-form.js  —  MarketHub Admin · Product Wizard
 *
 * Architecture rules:
 *  - All state changes via CSS class additions/removals only
 *  - No .style.* except ink left/width (requires DOM measurement — unavoidable)
 *  - No hardcoded currency symbols — read from [data-currency] on #wizRoot
 *  - No hardcoded HTML strings for server-rendered content
 *  - Event binding via data-attributes, never inline onXxx in markup
 *  - Dynamic variant cards use a <template> element from the DOM
 */
(function () {
  'use strict';

  /* ── Constants ─────────────────────────────────────────── */
  const W     = window._wiz || {};
  const N     = W.stepCount || 1;
  const STEPS = W.steps     || [];

  /* ── Root element & currency ────────────────────────────── */
  const ROOT     = document.getElementById('wizRoot');
  const CURRENCY = ROOT ? ROOT.dataset.currency || '₹' : '₹';
  const IS_ADMIN = ROOT ? ROOT.dataset.admin === 'true' : false;

  /* ── State ──────────────────────────────────────────────── */
  let curr = 0;

  /* ════════════════════════════════════════════════════════
     BOOT
     ════════════════════════════════════════════════════════ */
  document.addEventListener('DOMContentLoaded', function () {
    stampCurrencySymbols();
    stampVariantPrices();
    initInk();
    initFooter();
    bindNavButtons();
    bindStepButtons();
    bindSlug();
    bindRte();
    bindSeoPreview();
    bindCharCounters();
    bindTagChips();
    bindVariantList();
    bindStatusPicker();
    bindCurrencySelect();
    bindPricingHints();
    gotoStep(0, false);
  });

  /* ════════════════════════════════════════════════════════
     CURRENCY
     ════════════════════════════════════════════════════════ */

  function stampCurrencySymbols() {
    document.querySelectorAll('[data-currency-symbol]').forEach(el => {
      el.textContent = CURRENCY;
    });
  }

  function stampVariantPrices() {
    document.querySelectorAll('[data-price]').forEach(el => {
      el.textContent = CURRENCY + ' ' + parseFloat(el.dataset.price || '0').toFixed(2);
    });
  }

  /** When user changes currency, update all symbols and re-stamp prices */
  function bindCurrencySelect() {
    const sel = document.querySelector('[data-currency-select]');
    if (!sel) return;
    sel.addEventListener('change', function () {
      const sym = currencyCodeToSymbol(this.value);
      ROOT.dataset.currency = sym;
      document.querySelectorAll('[data-currency-symbol]').forEach(el => {
        el.textContent = sym;
      });
    });
  }

  function currencyCodeToSymbol(code) {
    return { INR: '₹', USD: '$', EUR: '€', GBP: '£' }[code] || code;
  }

  /* ════════════════════════════════════════════════════════
     WIZARD NAVIGATION
     ════════════════════════════════════════════════════════ */

  function bindNavButtons() {
    document.getElementById('btnNext')?.addEventListener('click', wizNext);
    document.getElementById('btnPrev')?.addEventListener('click', wizPrev);
  }

  function bindStepButtons() {
    document.querySelectorAll('.wiz-step').forEach(btn => {
      btn.addEventListener('click', () => gotoStep(parseInt(btn.dataset.idx)));
    });
    document.querySelectorAll('.wiz-dot').forEach(dot => {
      dot.addEventListener('click', () => gotoStep(parseInt(dot.dataset.idx)));
    });
  }

  function wizNext() {
    if (!validateStep(curr)) return;
    markDone(curr);
    gotoStep(curr + 1);
  }

  function wizPrev() {
    gotoStep(curr - 1);
  }

  window.gotoStep = function (idx, animate) {
    if (idx < 0 || idx >= N) return;
    if (animate === undefined) animate = true;

    const dir = idx > curr ? 'forward' : 'back';

    if (animate && idx !== curr) {
      animatePanel(curr, dir === 'forward' ? 'exit-left' : 'exit-right', () => {
        switchActivePanel(idx);
        animatePanel(idx, dir === 'forward' ? 'enter-right' : 'enter-left');
      });
    } else {
      switchActivePanel(idx);
    }

    curr = idx;
    syncStepBubbles();
    syncDots();
    syncFooterButtons();
    updateInk(animate);
  };

  function switchActivePanel(idx) {
    document.querySelectorAll('.wiz-panel').forEach(p => p.classList.remove('is-active'));
    const key   = STEPS[idx]?.key;
    const panel = document.getElementById('panel-' + key);
    if (panel) panel.classList.add('is-active');
  }

  function animatePanel(idx, direction, done) {
    const key   = STEPS[idx]?.key;
    const panel = document.getElementById('panel-' + key);
    if (!panel) { done?.(); return; }

    if (direction === 'exit-left' || direction === 'exit-right') {
      panel.classList.add('is-active', 'panel-' + direction);
      setTimeout(() => {
        panel.classList.remove('is-active', 'panel-' + direction);
        done?.();
      }, 220);
    } else {
      panel.classList.add('is-active', 'panel-' + direction);
      requestAnimationFrame(() => {
        requestAnimationFrame(() => panel.classList.remove('panel-' + direction));
      });
    }
  }

  function syncStepBubbles() {
    document.querySelectorAll('.wiz-step').forEach(btn => {
      const i = parseInt(btn.dataset.idx);
      btn.classList.toggle('is-active', i === curr);
    });
  }

  function syncDots() {
    document.querySelectorAll('.wiz-dot').forEach(dot => {
      const i = parseInt(dot.dataset.idx);
      dot.classList.toggle('is-active', i === curr);
    });
  }

  function syncFooterButtons() {
    const prev   = document.getElementById('btnPrev');
    const next   = document.getElementById('btnNext');
    const submit = document.getElementById('btnSubmit');
    const isLast = curr === N - 1;

    prev?.classList.toggle('is-hidden', curr === 0);
    next?.classList.toggle('wiz-submit--hidden', isLast);
    submit?.classList.toggle('wiz-submit--hidden', !isLast);
  }

  window.markDone = function (idx) {
    document.querySelectorAll('.wiz-step')[idx]?.classList.add('is-done');
    document.querySelector(`.wiz-dot[data-idx="${idx}"]`)?.classList.add('is-done');
  };

  /* ════════════════════════════════════════════════════════
     INK UNDERLINE  (only place where .style.* is used)
     ════════════════════════════════════════════════════════ */

  function initInk() {
    window.addEventListener('resize', () => updateInk(false));
  }

  function updateInk(animate) {
    const ink  = document.getElementById('wizInk');
    const rail = document.querySelector('.wiz-rail');
    const btn  = document.querySelectorAll('.wiz-step')[curr];
    if (!ink || !rail || !btn) return;

    if (!animate) {
      ink.classList.add('no-transition');
      requestAnimationFrame(() => ink.classList.remove('no-transition'));
    }

    const railRect = rail.getBoundingClientRect();
    const btnRect  = btn.getBoundingClientRect();
    ink.style.left  = (btnRect.left - railRect.left) + 'px';
    ink.style.width = btnRect.width + 'px';
  }

  function initFooter() {
    // Hide prev on load
    document.getElementById('btnPrev')?.classList.add('is-hidden');
    document.getElementById('btnSubmit')?.classList.add('wiz-submit--hidden');
  }

  /* ════════════════════════════════════════════════════════
     VALIDATION
     ════════════════════════════════════════════════════════ */

  function validateStep(idx) {
    const key = STEPS[idx]?.key;

    if (key === 'basic') {
      return (
        requireField('Name',       v => v.trim().length >= 2,
                     'Product name must be at least 2 characters') &&
        requireField('Slug',       v => v.trim().length >= 1,
                     'URL slug is required') &&
        requireField('CategoryId', v => v !== '0' && v !== '',
                     'Please select a category')
      );
    }
    if (key === 'pricing') {
      return requireField('BasePrice', v => parseFloat(v) > 0,
                          'Selling price must be greater than 0');
    }
    return true;
  }

  function requireField(id, test, message) {
    const el = document.getElementById(id)
            || document.querySelector(`[name="${id}"]`);
    if (!el) return true;
    const ok = test(el.value);
    if (!ok) {
      el.classList.add('is-invalid');
      el.focus();
      const errEl = document.querySelector(`[data-valmsg-for="${id}"], .field-error[for="${id}"]`);
      if (errEl) errEl.textContent = message;
      el.addEventListener('input', () => el.classList.remove('is-invalid'), { once: true });
    }
    return ok;
  }

  /* ════════════════════════════════════════════════════════
     SLUG
     ════════════════════════════════════════════════════════ */

  function bindSlug() {
    const nameEl = document.querySelector('[data-slug-source]');
    const slugEl = document.querySelector('[data-slug-target]');
    const regenBtn = document.querySelector('[data-regen-slug]');
    const counter  = document.getElementById('nameLen');
    if (!nameEl || !slugEl) return;

    nameEl.addEventListener('input', function () {
      if (counter) counter.textContent = this.value.length;
      if (!slugEl.dataset.manual) {
        slugEl.value = toSlug(this.value);
        pushSerpSlug(slugEl.value);
      }
    });

    slugEl.addEventListener('input', function () {
      this.dataset.manual = '1';
      pushSerpSlug(this.value);
    });

    regenBtn?.addEventListener('click', () => {
      delete slugEl.dataset.manual;
      slugEl.value = toSlug(nameEl.value);
      pushSerpSlug(slugEl.value);
    });
  }

  function toSlug(str) {
    return str.toLowerCase()
      .replace(/[^a-z0-9\s-]/g, '').trim()
      .replace(/\s+/g, '-').replace(/-+/g, '-')
      .substring(0, 350);
  }

  function pushSerpSlug(val) {
    const el = document.getElementById('serpSlug');
    if (el) el.textContent = val || 'product-slug';
  }

  /* ════════════════════════════════════════════════════════
     PRICING HINTS  (state via class, text via textContent)
     ════════════════════════════════════════════════════════ */

  function bindPricingHints() {
    document.querySelectorAll('[data-pricing-trigger]').forEach(el => {
      el.addEventListener('input', recalcPricing);
    });
  }

  function recalcPricing() {
    const base    = parseFloat(document.getElementById('BasePrice')?.value    || '0');
    const compare = parseFloat(document.getElementById('CompareAtPrice')?.value || '0');
    const cost    = parseFloat(document.getElementById('CostPrice')?.value    || '0');

    const dh = document.getElementById('discountHint');
    if (dh) {
      if (compare > base && compare > 0) {
        const pct = Math.round((1 - base / compare) * 100);
        dh.textContent = `↓ ${pct}% off — customers save ${CURRENCY}${(compare - base).toFixed(0)}`;
        dh.className   = 'form-hint hint-success';
      } else if (compare > 0) {
        dh.textContent = 'Compare-at price should be higher than the selling price';
        dh.className   = 'form-hint hint-error';
      } else {
        dh.textContent = '';
        dh.className   = 'form-hint';
      }
    }

    const mh = document.getElementById('marginHint');
    if (mh) {
      if (cost > 0 && base > 0) {
        const m = ((base - cost) / base * 100).toFixed(1);
        mh.textContent = `Margin: ${m}%`;
        mh.className   = parseFloat(m) >= 0 ? 'form-hint hint-success' : 'form-hint hint-error';
      } else {
        mh.textContent = '';
        mh.className   = 'form-hint';
      }
    }
  }

  /* ════════════════════════════════════════════════════════
     SEO PREVIEW
     ════════════════════════════════════════════════════════ */

  function bindSeoPreview() {
    document.querySelectorAll('[data-seo-preview]').forEach(el => {
      el.addEventListener('input', syncSeoPreview);
    });
  }

  function syncSeoPreview() {
    const title = document.getElementById('MetaTitle')?.value
               || document.getElementById('Name')?.value
               || 'Product Title';
    const desc  = document.getElementById('MetaDescription')?.value
               || 'Your meta description will appear here in Google search results.';
    const slug  = document.getElementById('Slug')?.value || 'product-slug';

    const t = document.getElementById('serpTitle');
    const d = document.getElementById('serpDesc');
    const s = document.getElementById('serpSlug');
    if (t) t.textContent = title;
    if (d) d.textContent = desc;
    if (s) s.textContent = slug;
  }

  /* ════════════════════════════════════════════════════════
     CHAR COUNTERS
     ════════════════════════════════════════════════════════ */

  function bindCharCounters() {
    wireCounter('MetaTitle',        'metaTitleLen',  60, 70);
    wireCounter('MetaDescription',  'metaDescLen',  150, 165);
    wireCounter('ShortDescription', 'shortDescLen', 400, 500);
  }

  function wireCounter(fieldId, counterId, warnAt, maxAt) {
    const field   = document.getElementById(fieldId);
    const counter = document.getElementById(counterId);
    if (!field || !counter) return;
    field.addEventListener('input', function () {
      const len = this.value.length;
      counter.textContent = len;
      counter.className   = len > maxAt ? 'counter-over'
                          : len > warnAt ? 'counter-warn' : '';
    });
  }

  /* ════════════════════════════════════════════════════════
     TAG CHIPS
     ════════════════════════════════════════════════════════ */

  function bindTagChips() {
    document.querySelectorAll('[data-tag-checkbox]').forEach(cb => {
      cb.addEventListener('change', function () {
        this.closest('.tag-chip')?.classList.toggle('is-on', this.checked);
      });
    });
  }

  /* ════════════════════════════════════════════════════════
     RTE
     ════════════════════════════════════════════════════════ */

  function bindRte() {
    const editor = document.querySelector('[data-rte-editor]');
    const hidden = document.querySelector('[data-rte-hidden]');
    if (!editor || !hidden) return;
    editor.addEventListener('input', () => { hidden.value = editor.innerHTML; });

    document.querySelectorAll('[data-rte-cmd]').forEach(btn => {
      btn.addEventListener('mousedown', function (e) {
        e.preventDefault();   // keep editor focus
        document.execCommand(this.dataset.rteCmd, false, null);
        hidden.value = editor.innerHTML;
        editor.focus();
      });
    });
  }

  /* ════════════════════════════════════════════════════════
     VARIANTS  — event delegation, no inline handlers
     ════════════════════════════════════════════════════════ */

  let _vi = document.querySelectorAll('[data-variant-card]').length;

  function bindVariantList() {
    const list = document.getElementById('variantList');
    if (!list) return;

    // Add variant buttons (toolbar + empty state)
    document.querySelectorAll('[data-variant-add]').forEach(btn => {
      btn.addEventListener('click', addVariant);
    });

    // Delegate toggle / remove / attr-add / attr-remove / live previews
    list.addEventListener('click', function (e) {
      const toggleBtn = e.target.closest('[data-variant-toggle]');
      const removeBtn = e.target.closest('[data-variant-remove]');
      const attrAdd   = e.target.closest('[data-attr-add]');
      const attrRem   = e.target.closest('[data-attr-remove]');
      const variantAddBtn = e.target.closest('[data-variant-add]');

      if (toggleBtn) toggleVariant(parseInt(toggleBtn.dataset.variantToggle));
      if (removeBtn) removeVariant(parseInt(removeBtn.dataset.variantRemove));
      if (attrAdd)   addAttribute(parseInt(attrAdd.dataset.attrAdd));
      if (attrRem)   attrRem.closest('.attr-row')?.remove();
      if (variantAddBtn) addVariant();
    });

    // Delegate name / price live preview
    list.addEventListener('input', function (e) {
      const namePrev  = e.target.dataset.variantNamePreview;
      const pricePrev = e.target.dataset.variantPricePreview;

      if (namePrev !== undefined) {
        const idx = parseInt(namePrev);
        const el  = document.getElementById('vname-' + idx);
        if (el) el.textContent = e.target.value || 'New Variant';
      }
      if (pricePrev !== undefined) {
        const idx = parseInt(pricePrev);
        const el  = document.getElementById('vprice-' + idx);
        if (el) el.textContent = ROOT.dataset.currency + ' ' +
                                 (parseFloat(e.target.value) || 0).toFixed(2);
      }
    });
  }

  function addVariant() {
    document.getElementById('variantEmpty')?.remove();
    const list = document.getElementById('variantList');
    if (!list) return;
    list.insertAdjacentHTML('beforeend', buildVariantHTML(_vi));
    // stamp currency symbols in the newly inserted card
    list.querySelectorAll(`#vc-${_vi} [data-currency-symbol]`).forEach(el => {
      el.textContent = ROOT.dataset.currency;
    });
    updateVariantCount(1);
    _vi++;
  }

  function removeVariant(idx) {
    document.getElementById('vc-' + idx)?.remove();
    const list    = document.getElementById('variantList');
    const remains = list?.querySelectorAll('[data-variant-card]').length ?? 0;
    updateVariantCount(-remains - 1 + remains);
    if (!remains) list.insertAdjacentHTML('beforeend', emptyVariantHTML());
    syncVariantCount();
  }

  function toggleVariant(idx) {
    const body = document.getElementById('vbody-' + idx);
    const chev = document.getElementById('vchev-' + idx);
    if (!body) return;
    body.classList.toggle('is-collapsed');
    chev?.classList.toggle('is-flipped');
  }

  function addAttribute(variantIdx) {
    const list = document.getElementById('attrs-' + variantIdx);
    if (!list) return;
    const j   = list.querySelectorAll('.attr-row').length;
    const row = document.createElement('div');
    row.className = 'attr-row';
    row.innerHTML = `
      <input type="text"
             name="Variants[${variantIdx}].Attributes[${j}].Name"
             class="form-input"
             placeholder="Name (e.g. Color)" />
      <input type="text"
             name="Variants[${variantIdx}].Attributes[${j}].Value"
             class="form-input"
             placeholder="Value (e.g. Red)" />
      <button type="button" class="row-btn danger" data-attr-remove>
        <svg width="12" height="12" fill="none" viewBox="0 0 24 24"
             stroke="currentColor" stroke-width="3">
          <path stroke-linecap="round" stroke-linejoin="round"
                d="M6 18L18 6M6 6l12 12"/>
        </svg>
      </button>`;
    list.appendChild(row);
  }

  function syncVariantCount() {
    const count = document.querySelectorAll('[data-variant-card]').length;
    const el    = document.getElementById('variantCount');
    if (el) el.textContent = count;
  }

  function updateVariantCount(delta) {
    const el = document.getElementById('variantCount');
    if (!el) return;
    const n = (parseInt(el.textContent) || 0) + delta;
    el.textContent = Math.max(0, n);
  }

  function emptyVariantHTML() {
    return `<div class="empty-state" id="variantEmpty" data-variant-empty>
      <div class="empty-icon">
        <svg width="32" height="32" fill="none" viewBox="0 0 24 24"
             stroke="currentColor" stroke-width="1.3">
          <path stroke-linecap="round" stroke-linejoin="round"
                d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0
                   01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2
                   2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"/>
        </svg>
      </div>
      <p class="empty-title">No variants yet</p>
      <p class="empty-desc">Add options like <em>Red / XL</em> or <em>Black / 256GB</em></p>
      <button type="button" class="btn btn-outline btn-sm" data-variant-add>
        Add First Variant
      </button>
    </div>`;
  }

  function buildVariantHTML(i) {
    const sym      = ROOT.dataset.currency;
    const adminRow = IS_ADMIN ? `
      <div class="form-cols-2">
        <div class="form-group">
          <label class="form-label">Cost Price <span class="admin-badge">Admin only</span></label>
          <div class="form-input-wrap">
            <span class="form-icon" data-currency-symbol>${sym}</span>
            <input type="number" name="Variants[${i}].CostPrice"
                   class="form-input form-input--icon" min="0" step="0.01" />
          </div>
        </div>
        <div class="form-group">
          <label class="form-label">Barcode <span class="opt">(optional)</span></label>
          <input type="text" name="Variants[${i}].Barcode" class="form-input" />
        </div>
      </div>` : '';

    return `
    <div class="variant-card" id="vc-${i}" data-variant-card data-index="${i}">
      <div class="variant-card-hd" data-variant-toggle="${i}">
        <div class="variant-drag-handle" aria-hidden="true">
          <svg width="11" height="11" fill="currentColor" viewBox="0 0 20 20">
            <circle cx="6"  cy="4"  r="1.5"/><circle cx="14" cy="4"  r="1.5"/>
            <circle cx="6"  cy="10" r="1.5"/><circle cx="14" cy="10" r="1.5"/>
            <circle cx="6"  cy="16" r="1.5"/><circle cx="14" cy="16" r="1.5"/>
          </svg>
        </div>
        <span class="variant-preview-name" id="vname-${i}">New Variant</span>
        <span class="variant-preview-price" id="vprice-${i}">${sym} 0.00</span>
        <div class="variant-card-actions">
          <button type="button" class="row-btn danger"
                  title="Remove variant" data-variant-remove="${i}">
            <svg width="13" height="13" fill="none" viewBox="0 0 24 24"
                 stroke="currentColor" stroke-width="2.5">
              <path stroke-linecap="round" stroke-linejoin="round"
                    d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0
                       01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4
                       a1 1 0 00-1 1v3M4 7h16"/>
            </svg>
          </button>
          <svg id="vchev-${i}" class="variant-chevron" width="14" height="14"
               fill="none" viewBox="0 0 24 24" stroke="currentColor"
               stroke-width="2.5" aria-hidden="true">
            <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7"/>
          </svg>
        </div>
      </div>

      <div class="variant-card-body" id="vbody-${i}">
        <div class="form-cols-2">
          <div class="form-group">
            <label class="form-label">Name <span class="req">*</span></label>
            <input type="text" name="Variants[${i}].Name"
                   class="form-input" placeholder="e.g. Red / XL"
                   data-variant-name-preview="${i}" />
          </div>
          <div class="form-group">
            <label class="form-label">Price <span class="req">*</span></label>
            <div class="form-input-wrap">
              <span class="form-icon" data-currency-symbol>${sym}</span>
              <input type="number" name="Variants[${i}].Price"
                     class="form-input form-input--icon" min="0" step="0.01"
                     data-variant-price-preview="${i}" />
            </div>
          </div>
        </div>
        <div class="form-cols-2">
          <div class="form-group">
            <label class="form-label">SKU <span class="opt">(optional)</span></label>
            <input type="text" name="Variants[${i}].Sku" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Compare-At <span class="opt">(optional)</span></label>
            <div class="form-input-wrap">
              <span class="form-icon" data-currency-symbol>${sym}</span>
              <input type="number" name="Variants[${i}].CompareAtPrice"
                     class="form-input form-input--icon" min="0" step="0.01" />
            </div>
          </div>
        </div>
        ${adminRow}
        <div class="attrs-section">
          <div class="attrs-hd">
            <span class="section-label attrs-label">Attributes</span>
            <button type="button" class="btn btn-ghost btn-xs"
                    data-attr-add="${i}">+ Add attribute</button>
          </div>
          <div class="attrs-list" id="attrs-${i}"></div>
        </div>
        <div class="toggle-row">
          <div class="toggle-info">
            <span class="toggle-label">Active</span>
            <span class="toggle-desc">Show this variant to customers</span>
          </div>
          <label class="toggle">
            <input type="checkbox" name="Variants[${i}].IsActive" value="true" checked />
            <span class="toggle-track"><span class="toggle-thumb"></span></span>
          </label>
        </div>
      </div>
    </div>`;
  }

  /* ════════════════════════════════════════════════════════
     STATUS PICKER
     ════════════════════════════════════════════════════════ */

  function bindStatusPicker() {
    // The rejection-reason group declares which status value triggers it
    // via data-rejection-trigger on the group element itself — no hardcoding
    const rejGroup = document.getElementById('rejReasonGroup');
    const trigger  = rejGroup?.dataset.rejectionTrigger ?? '';

    document.querySelectorAll('[data-status-radio]').forEach(radio => {
      radio.addEventListener('change', function () {
        // Sync selected appearance on all option labels
        document.querySelectorAll('[data-status-opt]').forEach(opt => {
          const inp = opt.querySelector('[data-status-radio]');
          opt.classList.toggle('is-on', inp?.value === this.value);
        });
        // Show/hide rejection reason group driven by data attribute
        if (rejGroup && trigger) {
          rejGroup.classList.toggle('is-hidden', this.value !== trigger);
        }
      });
    });
  }

})();
