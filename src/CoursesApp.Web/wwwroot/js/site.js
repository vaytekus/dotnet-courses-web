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

function debounce(fn, delay = 300) {
    let timer;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => fn(...args), delay);
    };
}

function apiCall(url, method = 'POST', data = null) {
    const options = { method };
    if (data !== null) {
        options.headers = { 'Content-Type': 'application/json' };
        options.body = JSON.stringify(data);
    }
    return fetch(url, options).then(r => r.json());
}

function enterEditMode(el) {
    el.querySelectorAll('.view-mode').forEach(e => e.classList.add('d-none'));
    el.querySelectorAll('.edit-mode').forEach(e => e.classList.remove('d-none'));
}

function exitEditMode(el) {
    el.querySelectorAll('.view-mode').forEach(e => e.classList.remove('d-none'));
    el.querySelectorAll('.edit-mode').forEach(e => e.classList.add('d-none'));
}