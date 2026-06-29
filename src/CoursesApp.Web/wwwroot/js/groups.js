let currentPage = 1;
let debounceTimer;

function searchGroups(page = 1) {
    currentPage = page;
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
        const search = document.getElementById('search-input')?.value.trim() ?? '';
        const courseId = document.getElementById('filter-course')?.value ?? '';
        const filter = document.getElementById('filter-students')?.value ?? '0';
        const params = new URLSearchParams({ search, courseId, filter, page: currentPage });
        fetch(`/groups/search?${params}`)
            .then(r => r.text())
            .then(html => {
                document.querySelector('#groups-accordion').innerHTML = html;
            });
    }, 300);
}

if (document.getElementById('groups-accordion')) {
    document.addEventListener('click', function (e) {

        if (e.target.classList.contains('btn-edit')) {
            const item = e.target.closest('li[data-id]');
            item.querySelectorAll('.view-mode').forEach(el => el.classList.add('d-none'));
            item.querySelectorAll('.edit-mode').forEach(el => el.classList.remove('d-none'));

            const nameInput = item.querySelector('input[name="groupName"]');
            const teacherSelect = item.querySelector('select[name="teacherId"]');
            const saveBtn = item.querySelector('.btn-save');

            item.dataset.originalName = nameInput.value;
            item.dataset.originalTeacherId = teacherSelect.value;

            saveBtn.disabled = true;

            function validateEdit() {
                const nameChanged = nameInput.value !== item.dataset.originalName;
                const teacherChanged = teacherSelect.value !== item.dataset.originalTeacherId;
                const nameFilled = nameInput.value.trim() !== '';
                saveBtn.disabled = !(nameFilled && (nameChanged || teacherChanged));
            }

            nameInput.addEventListener('input', validateEdit);
            teacherSelect.addEventListener('change', validateEdit);
        }

        if (e.target.classList.contains('btn-cancel')) {
            const item = e.target.closest('li[data-id]');
            item.querySelector('input[name="groupName"]').value = item.dataset.originalName;
            item.querySelector('select[name="teacherId"]').value = item.dataset.originalTeacherId;
            item.querySelectorAll('.view-mode').forEach(el => el.classList.remove('d-none'));
            item.querySelectorAll('.edit-mode').forEach(el => el.classList.add('d-none'));
        }

        if (e.target.classList.contains('btn-save')) {
            const item = e.target.closest('li[data-id]');
            const id = item.dataset.id;
            const name = item.querySelector('input[name="groupName"]').value;
            const teacherId = item.querySelector('select[name="teacherId"]').value || null;

            fetch('/groups/edit', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id, name, teacherId })
            }).then(r => r.json()).then(res => {
                if (res.success) searchGroups(currentPage);
            });
        }

        if (e.target.classList.contains('btn-delete')) {
            const item = e.target.closest('li[data-id]');
            const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
            document.getElementById('btn-confirm-delete').onclick = () => {
                fetch(`/groups/delete/${item.dataset.id}`, { method: 'DELETE' })
                    .then(r => r.json())
                    .then(res => {
                        modal.hide();
                        if (res.success) {
                            searchGroups(currentPage);
                        } else {
                            alert(res.message);
                        }
                    });
            };
            modal.show();
        }

        if (e.target.classList.contains('btn-clear-students')) {
            const item = e.target.closest('li[data-id]');
            const modal = new bootstrap.Modal(document.getElementById('clearStudentsModal'));
            document.getElementById('btn-confirm-clear').onclick = () => {
                fetch(`/groups/clearstudents?id=${item.dataset.id}`, { method: 'POST' })
                    .then(r => r.json())
                    .then(res => {
                        if (res.success) {
                            modal.hide();
                            searchGroups(currentPage);
                        }
                    });
            };
            modal.show();
        }

        if (e.target.classList.contains('btn-page')) {
            const page = parseInt(e.target.dataset.page);
            if (!isNaN(page)) searchGroups(page);
        }
    });
}

const addForm = document.getElementById('add-group-form');
if (addForm) {
    const nameInput = document.getElementById('groupName');
    const courseSelect = document.getElementById('selectCourse');
    const teacherSelect = document.getElementById('selectTeacher');
    const saveBtn = document.getElementById('btn-add-save');
    const cancelBtn = document.getElementById('btn-add-cancel');
    const showFormBtn = document.getElementById('btn-add-group');
    let isShowForm = false;

    function validateAddForm() {
        saveBtn.disabled = nameInput.value.trim() === '' || courseSelect.value === '';
    }

    function toggleForm() {
        const isHiding = isShowForm;
        addForm.querySelectorAll('.input-group').forEach(el => el.classList.toggle('d-none', isHiding));
        addForm.querySelectorAll('.button-holder').forEach(el => el.classList.toggle('d-none', !isHiding));
        isShowForm = !isShowForm;
    }

    function resetForm() {
        addForm.reset();
        saveBtn.disabled = true;
        if (isShowForm) toggleForm();
    }

    function saveGroup() {
        fetch('/groups/add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                name: nameInput.value,
                courseId: courseSelect.value,
                teacherId: teacherSelect.value || null
            })
        }).then(r => r.json()).then(res => {
            if (res.success) {
                resetForm();
                searchGroups(currentPage);
            }
        });
    }

    nameInput.addEventListener('input', validateAddForm);
    courseSelect.addEventListener('change', validateAddForm);
    showFormBtn.addEventListener('click', toggleForm);
    cancelBtn.addEventListener('click', resetForm);
    saveBtn.addEventListener('click', saveGroup);
}

const searchForm = document.getElementById('search-group-form');
if (searchForm) {
    document.getElementById('search-input').addEventListener('input', () => searchGroups(1));
    document.getElementById('filter-course').addEventListener('change', () => searchGroups(1));
    document.getElementById('filter-students').addEventListener('change', () => searchGroups(1));
}