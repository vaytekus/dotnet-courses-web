document.addEventListener('show.bs.collapse', function (e) {
    const collapseEl = e.target;
    const groupId = collapseEl.dataset.groupId;
    if (!groupId) return;

    const container = collapseEl.querySelector('.students-container');
    if (!container || container.innerHTML.trim()) return;

    e.preventDefault();

    container.innerHTML = `
        <div class="d-flex justify-content-center py-3">
            <div class="spinner-border spinner-border-sm text-secondary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>`;

    bootstrap.Collapse.getOrCreateInstance(collapseEl).show();

    fetch(`/students/getstudent?groupId=${groupId}`)
        .then(res => res.text())
        .then(html => { container.innerHTML = html; });
});