let _studentTableDirty = false;
let _pendingSaveStudentId = null;

connection.on("StudentAdded", () => searchStudents(currentPage));

connection.on("StudentDeleted", (studentId) => {
    const editingRow = document.querySelector('#students-table .edit-mode:not(.d-none)')?.closest('tr');
    if (editingRow) {
        if (editingRow.dataset.id === studentId) {
            exitEditMode(editingRow);
            editingRow.remove();
        } else {
            _studentTableDirty = true;
        }
        return;
    }
    searchStudents(currentPage);
});

connection.on("GroupDeleted", (groupId, studentIds) => {
    if (!studentIds?.length) return;
    const hasVisible = studentIds.some(id => document.querySelector(`#students-table tr[data-id="${id}"]`));
    if (hasVisible) searchStudents(currentPage);
});

connection.on("StudentsCleared", (groupId, studentIds) => {
    if (!studentIds?.length) return;
    const hasVisible = studentIds.some(id => document.querySelector(`#students-table tr[data-id="${id}"]`));
    if (hasVisible) searchStudents(currentPage);
});

connection.on("StudentUpdated", (studentId, firstName, lastName, groupId) => {
    if (_pendingSaveStudentId === studentId) return;
    const row = document.querySelector(`tr[data-id="${studentId}"]`);
    if (!row) return;
    if (row.querySelector('.edit-mode:not(.d-none)')) {
        alert('This student was updated by another user. Your changes may conflict.');
        return;
    }
    row.querySelector('td:nth-child(2) .view-mode').textContent = firstName;
    row.querySelector('td:nth-child(3) .view-mode').textContent = lastName;
    const select = row.querySelector('select[name="groupId"]');
    if (select) {
        select.value = groupId;
        row.querySelector('td:nth-child(4) .view-mode').textContent = select.options[select.selectedIndex]?.text ?? '';
    }
});

let currentPage = 1;
let searchAbortController = null;
let currentSort = window.initialSort ?? { key: 'lastName', desc: false };

function updateSortArrows() {
    document.querySelectorAll('#students-table thead th[data-sort-key]').forEach(th => {
        const arrow = th.querySelector('.sort-arrow');
        if (!arrow) return;
        if (th.dataset.sortKey === currentSort.key) {
            arrow.textContent = currentSort.desc ? ' ▼' : ' ▲';
        } else {
            arrow.textContent = '';
        }
    });
}

const _doSearchStudents = debounce(() => {
    if (searchAbortController) searchAbortController.abort();
    searchAbortController = new AbortController();
    const search = document.getElementById('search-input')?.value.trim() ?? '';
    const groupId = document.getElementById('filteringByGroup')?.value ?? '';
    const params = new URLSearchParams({
        search,
        groupId,
        sortKey: currentSort.key,
        sortDesc: currentSort.desc,
        page: currentPage
    });
    fetch(`/students/search?${params}`, { signal: searchAbortController.signal })
        .then(r => r.text())
        .then(html => { document.querySelector('#students-table tbody').innerHTML = html; })
        .catch(err => { if (err.name !== 'AbortError') console.error(err); });
});

function searchStudents(page = 1) {
    currentPage = page;
    _doSearchStudents();
}

if (document.getElementById('students-table')) {
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('btn-edit')) {
            const row = e.target.closest('tr');
            enterEditMode(row);
            const saveBtn = row.querySelector('.btn-save');
            const firstNameInput = row.querySelector('input[name="firstName"]');
            const lastNameInput = row.querySelector('input[name="lastName"]');
            const groupSelect = row.querySelector('select[name="groupId"]');
            const originalFirstName = firstNameInput.value;
            const originalLastName = lastNameInput.value;
            const originalGroupId = groupSelect.value;
            row.dataset.originalFirstName = originalFirstName;
            row.dataset.originalLastName = originalLastName;
            row.dataset.originalGroupId = originalGroupId;
            saveBtn.disabled = true;
            function validateEdit() {
                const changed = firstNameInput.value !== originalFirstName
                    || lastNameInput.value !== originalLastName
                    || groupSelect.value !== originalGroupId;
                const filled = firstNameInput.value.trim() !== '' && lastNameInput.value.trim() !== '';
                saveBtn.disabled = !(changed && filled);
            }
            firstNameInput.addEventListener('input', validateEdit);
            lastNameInput.addEventListener('input', validateEdit);
            groupSelect.addEventListener('change', validateEdit);
        }

        if (e.target.classList.contains('btn-cancel')) {
            const row = e.target.closest('tr');
            row.querySelector('input[name="firstName"]').value = row.dataset.originalFirstName;
            row.querySelector('input[name="lastName"]').value = row.dataset.originalLastName;
            row.querySelector('select[name="groupId"]').value = row.dataset.originalGroupId;
            exitEditMode(row);
            if (_studentTableDirty) { _studentTableDirty = false; searchStudents(currentPage); }
        }

        if (e.target.classList.contains('btn-save')) {
            const row = e.target.closest('tr');
            const id = row.dataset.id;
            const firstName = row.querySelector('input[name="firstName"]').value;
            const lastName = row.querySelector('input[name="lastName"]').value;
            const groupId = row.querySelector('select[name="groupId"]').value;
            _pendingSaveStudentId = id;
            apiCall('/students/edit', 'POST', { id, firstName, lastName, groupId })
                .then(res => {
                    if (res.success) {
                        _pendingSaveStudentId = null;
                        row.querySelector('td:nth-child(2) .view-mode').textContent = firstName;
                        row.querySelector('td:nth-child(3) .view-mode').textContent = lastName;
                        const select = row.querySelector('select[name="groupId"]');
                        row.querySelector('td:nth-child(4) .view-mode').textContent = select.options[select.selectedIndex].text;
                        exitEditMode(row);
                        if (_studentTableDirty) { _studentTableDirty = false; searchStudents(currentPage); }
                    } else {
                        _pendingSaveStudentId = null;
                    }
                });
        }

        if (e.target.classList.contains('btn-delete')) {
            const row = e.target.closest('tr');
            document.getElementById('delete-target-name').textContent =
                `${row.dataset.firstName} ${row.dataset.lastName}`;
            const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
            document.getElementById('btn-confirm-delete').onclick = () => {
                apiCall(`/students/delete/${row.dataset.id}?groupId=${row.dataset.groupId}`, 'DELETE')
                    .then(res => { if (res.success) { modal.hide(); row.remove(); } });
            };
            modal.show();
        }

        if (e.target.classList.contains('btn-page')) {
            searchStudents(parseInt(e.target.dataset.page));
        }
    });

    document.querySelectorAll('#students-table thead th[data-sort-key]').forEach(th => {
        th.addEventListener('click', () => {
            const key = th.dataset.sortKey;
            if (currentSort.key === key) {
                currentSort.desc = !currentSort.desc;
            } else {
                currentSort.key = key;
                currentSort.desc = false;
            }
            updateSortArrows();
            searchStudents(currentPage);
        });
    });

    updateSortArrows();
}

const addForm = document.getElementById('add-student-form');
if (addForm) {
    const firstName = document.getElementById('studentFirstName');
    const lastName = document.getElementById('studentLastName');
    const group = document.getElementById('selectStudentGroup');
    const saveBtn = document.getElementById('btn-add-save');
    const cancelBtn = document.getElementById('btn-add-cancel');
    const showFormBtn = document.getElementById('btn-add-student');
    let isShowForm = false;

    function validateAddForm() {
        saveBtn.disabled = firstName.value.trim() === '' || lastName.value.trim() === '' || group.value === '';
    }

    function toggleForm() {
        if (!isShowForm) {
            addForm.querySelectorAll('.input-group').forEach(el => el.classList.remove('d-none'));
            addForm.querySelectorAll('.button-holder').forEach(el => el.classList.add('d-none'));
        } else {
            addForm.querySelectorAll('.input-group').forEach(el => el.classList.add('d-none'));
            addForm.querySelectorAll('.button-holder').forEach(el => el.classList.remove('d-none'));
        }
        isShowForm = !isShowForm;
    }

    function saveStudent() {
        saveBtn.disabled = true;
        apiCall('/students/add', 'POST', { firstName: firstName.value, lastName: lastName.value, groupId: group.value })
            .then(res => {
                if (res.success) { addForm.reset(); toggleForm(); }
                else { saveBtn.disabled = false; }
            }).catch(() => { saveBtn.disabled = false; });
    }

    firstName.addEventListener('input', validateAddForm);
    lastName.addEventListener('input', validateAddForm);
    group.addEventListener('change', validateAddForm);
    showFormBtn.addEventListener('click', toggleForm);
    cancelBtn.addEventListener('click', () => { addForm.reset(); toggleForm(); });
    saveBtn.addEventListener('click', saveStudent);
}

const searchForm = document.getElementById('search-student-form');
if (searchForm) {
    document.getElementById('search-input').addEventListener('input', () => searchStudents(1));
    document.getElementById('filteringByGroup').addEventListener('change', () => searchStudents(1));
}
