// shared state
let currentPage = 1;
let debounceTimer;

function searchStudents(page = 1) {
    currentPage = page;
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
        const searchInput = document.getElementById('search-input');
        const filteringByGroup = document.getElementById('filteringByGroup');
        const search = searchInput ? searchInput.value.trim() : '';
        const groupId = filteringByGroup ? filteringByGroup.value : '';
        const params = new URLSearchParams({ search, groupId, page: currentPage });
        fetch(`/students/search?${params}`)
            .then(r => r.text())
            .then(html => {
                document.querySelector('#students-table tbody').innerHTML = html;
            });
    }, 300);
}

// edit, delete students
if (document.getElementById('students-table')) {
    document.addEventListener('click', function(e) {
        // Edit
        if (e.target.classList.contains('btn-edit')) {
            const row = e.target.closest('tr');
            row.querySelectorAll('.view-mode').forEach(el => el.classList.add('d-none'));
            row.querySelectorAll('.edit-mode').forEach(el => el.classList.remove('d-none'));

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
                const filled = firstNameInput.value.trim() !== ''
                    && lastNameInput.value.trim() !== '';
                saveBtn.disabled = !(changed && filled);
            }

            firstNameInput.addEventListener('input', validateEdit);
            lastNameInput.addEventListener('input', validateEdit);
            groupSelect.addEventListener('change', validateEdit);
        }

        // Cancel
        if (e.target.classList.contains('btn-cancel')) {
            const row = e.target.closest('tr');
            row.querySelector('input[name="firstName"]').value = row.dataset.originalFirstName;
            row.querySelector('input[name="lastName"]').value = row.dataset.originalLastName;
            row.querySelector('select[name="groupId"]').value = row.dataset.originalGroupId;
            row.querySelectorAll('.view-mode').forEach(el => el.classList.remove('d-none'));
            row.querySelectorAll('.edit-mode').forEach(el => el.classList.add('d-none'));
        }

        // Save
        if (e.target.classList.contains('btn-save')) {
            const row = e.target.closest('tr');
            const id = row.dataset.id;
            const firstName = row.querySelector('input[name="firstName"]').value;
            const lastName = row.querySelector('input[name="lastName"]').value;
            const groupId = row.querySelector('select[name="groupId"]').value;

            fetch('/students/edit', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id, firstName, lastName, groupId })
            }).then(r => r.json()).then(res => {
                if (res.success) {
                    row.querySelector('td:nth-child(2) .view-mode').textContent = firstName;
                    row.querySelector('td:nth-child(3) .view-mode').textContent = lastName;
                    const select = row.querySelector('select[name="groupId"]');
                    const groupName = select.options[select.selectedIndex].text;
                    row.querySelector('td:nth-child(4) .view-mode').textContent = groupName;
                    row.querySelectorAll('.view-mode').forEach(el => el.classList.remove('d-none'));
                    row.querySelectorAll('.edit-mode').forEach(el => el.classList.add('d-none'));
                }
            });
        }

        // Delete
        if (e.target.classList.contains('btn-delete')) {
            const row = e.target.closest('tr');
            const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
            document.getElementById('btn-confirm-delete').onclick = () => {
                fetch(`/students/delete/${row.dataset.id}`, {
                    method: 'DELETE',
                    headers: { 'Content-Type': 'application/json' },
                }).then(r => r.json()).then(res => {
                    if (res.success) {
                        modal.hide();
                        searchStudents(currentPage);
                    }
                });
            };
            modal.show();
        }

        // Pagination
        if (e.target.classList.contains('btn-page')) {
            searchStudents(parseInt(e.target.dataset.page));
        }
    });
}

// Create student
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
        const valid = firstName.value.trim() !== ''
            && lastName.value.trim() !== ''
            && group.value !== '';
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

    function saveStudent() {
        const firstNameValue = firstName.value;
        const lastNameValue = lastName.value;
        const groupId = group.value;

        fetch('/students/add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ firstName: firstNameValue, lastName: lastNameValue, groupId })
        }).then(r => r.json()).then(res => {
            if (res.success) {
                resetForm();
                searchStudents(currentPage);
            }
        });
    }

    firstName.addEventListener('input', validateAddForm);
    lastName.addEventListener('input', validateAddForm);
    group.addEventListener('input', validateAddForm);
    showFormBtn.addEventListener('click', toggleForm);
    cancelBtn.addEventListener('click', resetForm);
    saveBtn.addEventListener('click', saveStudent);
}

// Search student
const searchForm = document.getElementById('search-student-form');
if (searchForm) {
    const searchInput = document.getElementById('search-input');
    const filteringByGroup = document.getElementById('filteringByGroup');

    searchInput.addEventListener('input', () => searchStudents(1));
    filteringByGroup.addEventListener('change', () => searchStudents(1));
}
