// Sidebar toggle
function toggleSidebar() {
    const s = document.getElementById('sidebar');
    const o = document.getElementById('overlay');
    const open = s.classList.toggle('open');
    o.classList.toggle('show', open);
    document.body.style.overflow = open ? 'hidden' : '';
}
function closeSidebar() {
    document.getElementById('sidebar')?.classList.remove('open');
    document.getElementById('overlay')?.classList.remove('show');
    document.body.style.overflow = '';
}
window.addEventListener('resize', () => {
    if (window.innerWidth > 860) closeSidebar();
});

// Auto-dismiss toasts
const toast = document.getElementById('toast');
if (toast) {
    setTimeout(() => {
        toast.style.transition = 'opacity .4s, transform .4s';
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(8px)';
        setTimeout(() => toast.remove(), 400);
    }, 4000);
}

// Slug auto-generation from name inputs
document.querySelectorAll('input[name="Name"]').forEach(nameInput => {
    const slugInput = document.querySelector('input[name="Slug"]');
    if (!slugInput) return;
    nameInput.addEventListener('input', () => {
        if (slugInput.dataset.manuallyEdited) return;
        slugInput.value = nameInput.value
            .toLowerCase()
            .replace(/[^a-z0-9\s-]/g, '')
            .replace(/\s+/g, '-')
            .replace(/-+/g, '-')
            .replace(/^-|-$/g, '');
    });
    slugInput.addEventListener('input', () => {
        slugInput.dataset.manuallyEdited = 'true';
    });
});

// ── Delete modal ──────────────────────────────────────────────
function openDelModal(action, name) {
    document.getElementById('delModalDesc').textContent =
        name ? `Delete "${name}"? This cannot be undone.` : '';
    document.getElementById('delForm').action = action;
    document.getElementById('delModal').style.display = 'flex';
}

function closeDelModal() {
    document.getElementById('delModal').style.display = 'none';
}