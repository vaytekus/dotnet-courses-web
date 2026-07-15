const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/app")
    .withAutomaticReconnect()
    .build();

connection.start().catch(err => console.error("SignalR:", err));

document.addEventListener('show.bs.collapse', function (e) {
    const collapseEl = e.target;
    const groupId = collapseEl.dataset.groupId;
    if (!groupId) return;

    const container = collapseEl.querySelector('.students-container');
    if (!container) return;

    const isDirty = collapseEl.dataset.dirty === 'true';
    if (!isDirty && container.innerHTML.trim()) return;

    delete collapseEl.dataset.dirty;
    e.preventDefault();

    container.innerHTML = `
        <div class="d-flex justify-content-center py-3">
            <div class="spinner-border spinner-border-sm text-secondary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>`;

    bootstrap.Collapse.getOrCreateInstance(collapseEl).show();

    const readOnly = collapseEl.dataset.readonly === 'true';
    fetch(buildStudentsUrl(groupId, 1, 'lastName', false, readOnly))
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

function showError(message) {
    document.getElementById('error-modal-message').textContent = message;
    new bootstrap.Modal(document.getElementById('errorModal')).show();
}

function enterEditMode(el) {
    el.querySelectorAll('.view-mode').forEach(e => e.classList.add('d-none'));
    el.querySelectorAll('.edit-mode').forEach(e => e.classList.remove('d-none'));
}

function exitEditMode(el) {
    el.querySelectorAll('.view-mode').forEach(e => e.classList.remove('d-none'));
    el.querySelectorAll('.edit-mode').forEach(e => e.classList.add('d-none'));
}

function getStudentsSort(collapse) {
    const page = collapse.querySelector('.students-page');
    return {
        key: page?.dataset.currentSortKey ?? 'lastName',
        desc: page?.dataset.currentSortDesc === 'true',
        page: page?.dataset.currentPage ?? '1'
    };
}

function buildStudentsUrl(groupId, page, sortKey, sortDesc, readOnly = false) {
    let url = `/students/getstudent?groupId=${groupId}&page=${page}&sortKey=${sortKey}&sortDesc=${sortDesc}`;
    if (readOnly) url += `&readOnly=true`;
    return url;
}

document.addEventListener('click', function (e) {
    if (e.target.classList.contains('btn-page-students')) {
        const page = parseInt(e.target.dataset.page);
        if (isNaN(page)) return;
        const collapse = e.target.closest('[data-group-id]');
        if (!collapse) return;
        const container = collapse.querySelector('.students-container');
        const s = getStudentsSort(collapse);
        const readOnly = collapse.dataset.readonly === 'true';
        fetch(buildStudentsUrl(collapse.dataset.groupId, page, s.key, s.desc, readOnly))
            .then(r => r.text())
            .then(html => { container.innerHTML = html; });
    }

    const sortTh = e.target.closest('.students-page thead th.sortable');
    if (sortTh) {
        const collapse = sortTh.closest('[data-group-id]');
        if (!collapse) return;
        const container = collapse.querySelector('.students-container');
        const s = getStudentsSort(collapse);
        const newKey = sortTh.dataset.sortKey;
        const newDesc = (s.key === newKey) ? !s.desc : false;
        const readOnly = collapse.dataset.readonly === 'true';
        fetch(buildStudentsUrl(collapse.dataset.groupId, s.page, newKey, newDesc, readOnly))
            .then(r => r.text())
            .then(html => { container.innerHTML = html; });
    }
});

document.addEventListener('click', function (e) {
    if (e.target.closest('button, input, select, a, .edit-mode')) return;

    const row = e.target.closest('.details-row');
    if (!row) return;

    const targetSel = row.dataset.detailsTarget;
    if (!targetSel) return;

    const details = document.querySelector(targetSel);
    if (!details) return;

    details.classList.toggle('d-none');
    const chevron = row.querySelector('.chevron');
    if (chevron) chevron.textContent = details.classList.contains('d-none') ? '▸' : '▾';
});