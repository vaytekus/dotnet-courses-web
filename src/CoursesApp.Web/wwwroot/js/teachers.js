let _teacherTableDirty = false;
let _pendingSaveTeacherId = null;

connection.on("TeacherAdded", () => searchTeachers(currentPage));

connection.on("TeacherDeleted", (teacherId) => {
    const editingRow = document.querySelector('#teachers-table .edit-mode:not(.d-none)')?.closest('tr');
    if (editingRow) {
        if (editingRow.dataset.id === teacherId) {
            exitEditMode(editingRow);
            editingRow.remove();
        } else {
            _teacherTableDirty = true;
        }
        return;
    }
    searchTeachers(currentPage);
});

connection.on("TeacherUpdated", (teacherId, firstName, lastName) => {
    if (_pendingSaveTeacherId === teacherId) return;
    const row = document.querySelector(`tr[data-id="${teacherId}"]`);
    if (!row) return;
    if (row.querySelector('.edit-mode:not(.d-none)')) {
        alert('This teacher was updated by another user. Your changes may conflict.');
        return;
    }
    row.querySelector('td:nth-child(2) .view-mode').textContent = firstName;
    row.querySelector('td:nth-child(3) .view-mode').textContent = lastName;
});

let currentPage = 1;
let debounceTimer;

function searchTeachers(page = 1) {
    currentPage = page;
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
        const searchInput = document.getElementById('search-input');
        const search = searchInput ? searchInput.value.trim() : '';
        const params = new URLSearchParams({ search, page: currentPage });
        fetch(`/teachers/search?${params}`)
            .then(r => r.text())
            .then(html => {
                document.querySelector('#teachers-table tbody').innerHTML = html;
            });
    }, 300);
}

if (document.getElementById('teachers-table')) {
    document.addEventListener('click', function(e) {
        // Edit
        if (e.target.classList.contains('btn-edit')) {
            const row = e.target.closest('tr');
            row.querySelectorAll('.view-mode').forEach(el => el.classList.add('d-none'));
            row.querySelectorAll('.edit-mode').forEach(el => el.classList.remove('d-none'));

            const saveBtn = row.querySelector('.btn-save');
            const firstNameInput = row.querySelector('input[name="firstName"]');
            const lastNameInput = row.querySelector('input[name="lastName"]');

            const originalFirstName = firstNameInput.value;
            const originalLastName = lastNameInput.value;

            row.dataset.originalFirstName = originalFirstName;
            row.dataset.originalLastName = originalLastName;

            saveBtn.disabled = true;

            function validateEdit() {
                const changed = firstNameInput.value !== originalFirstName
                    || lastNameInput.value !== originalLastName;
                const filled = firstNameInput.value.trim() !== ''
                    && lastNameInput.value.trim() !== '';
                saveBtn.disabled = !(changed && filled);
            }

            firstNameInput.addEventListener('input', validateEdit);
            lastNameInput.addEventListener('input', validateEdit);
        }

        // Cancel
        if (e.target.classList.contains('btn-cancel')) {
            const row = e.target.closest('tr');
            row.querySelector('input[name="firstName"]').value = row.dataset.originalFirstName;
            row.querySelector('input[name="lastName"]').value = row.dataset.originalLastName;
            exitEditMode(row);
            if (_teacherTableDirty) { _teacherTableDirty = false; searchTeachers(currentPage); }
        }

        // Save
        if (e.target.classList.contains('btn-save')) {
            const row = e.target.closest('tr');
            const id = row.dataset.id;
            const firstName = row.querySelector('input[name="firstName"]').value;
            const lastName = row.querySelector('input[name="lastName"]').value;

            _pendingSaveTeacherId = id;
            fetch('/teachers/edit', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id, firstName, lastName })
            }).then(r => r.json()).then(res => {
                if (res.success) {
                    _pendingSaveTeacherId = null;
                    row.querySelector('td:nth-child(2) .view-mode').textContent = firstName;
                    row.querySelector('td:nth-child(3) .view-mode').textContent = lastName;
                    exitEditMode(row);
                    if (_teacherTableDirty) { _teacherTableDirty = false; searchTeachers(currentPage); }
                } else { _pendingSaveTeacherId = null; }
            });
        }

        // Pagination
        if (e.target.classList.contains('btn-page')) {
            const page = parseInt(e.target.dataset.page);
            if (!isNaN(page)) searchTeachers(page);
        }

        // Delete
        if (e.target.classList.contains('btn-delete')) {
            const row = e.target.closest('tr');
            const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
            document.getElementById('btn-confirm-delete').onclick = () => {
                fetch(`/teachers/delete/${row.dataset.id}`, {
                    method: 'DELETE',
                    headers: { 'Content-Type': 'application/json' },
                }).then(r => r.json()).then(res => {
                    if (res.success) { modal.hide(); row.remove(); }
                });
            };
            modal.show();
        }
    });
}

// Create teacher
const addForm = document.getElementById('add-teacher-form');
if (addForm) {
    const firstName = document.getElementById('teacherFirstName');
    const lastName = document.getElementById('teacherLastName');
    const saveBtn = document.getElementById('btn-add-save');
    const cancelBtn = document.getElementById('btn-add-cancel');
    const showFormBtn = document.getElementById('btn-add-teacher');
    let isShowForm = false;

    function validateAddForm() {
        const valid = firstName.value.trim() !== ''
            && lastName.value.trim() !== '';
        saveBtn.disabled = !valid;
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

    function resetForm() {
        addForm.reset();
        toggleForm();
    }

    function saveTeacher() {
        const firstNameValue = firstName.value;
        const lastNameValue = lastName.value;

        fetch('/teachers/add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ firstName: firstNameValue, lastName: lastNameValue })
        }).then(r => r.json()).then(res => {
            if (res.success) { resetForm(); }
        });
    }

    firstName.addEventListener('input', validateAddForm);
    lastName.addEventListener('input', validateAddForm);
    showFormBtn.addEventListener('click', toggleForm);
    cancelBtn.addEventListener('click', resetForm);
    saveBtn.addEventListener('click', saveTeacher);
}

const searchForm = document.getElementById('search-teacher-form');
if (searchForm) {
    const searchInput = document.getElementById('search-input');
    searchInput.addEventListener('input', () => searchTeachers(1));
}