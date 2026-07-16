connection.on("TeacherAdded", (teacherId, firstName, lastName) => {
    const fullName = `${firstName} ${lastName}`;
    document.querySelectorAll('select[name="teacherId"], #selectTeacher')
        .forEach(select => {
            const opt = document.createElement('option');
            opt.value = teacherId;
            opt.textContent = fullName;
            select.appendChild(opt);
        });
});

connection.on("TeacherUpdated", (teacherId, firstName, lastName) => {
    const fullName = `${firstName} ${lastName}`;
    document.querySelectorAll(`[data-teacher-id="${teacherId}"]`)
        .forEach(el => el.textContent = fullName);
    document.querySelectorAll(`select[name="teacherId"] option[value="${teacherId}"], #selectTeacher option[value="${teacherId}"]`)
        .forEach(opt => opt.textContent = fullName);
});

connection.on("TeacherDeleted", (teacherId) => {
    document.querySelectorAll(`[data-teacher-id="${teacherId}"]`)
        .forEach(el => { el.textContent = '—'; el.removeAttribute('data-teacher-id'); });
    document.querySelectorAll(`select[name="teacherId"] option[value="${teacherId}"], #selectTeacher option[value="${teacherId}"]`)
        .forEach(opt => {
            const select = opt.parentElement;
            const wasSelected = opt.selected;
            opt.remove();
            if (wasSelected) select.value = '';
        });
});

connection.on("StudentUpdated", (studentId, firstName, lastName, groupId) => {
    const collapse = document.getElementById(`collapse-${groupId}`);
    if (!collapse) return;

    if (collapse.classList.contains('show')) {
        const row = collapse.querySelector(`tr[data-id="${studentId}"]`);
        if (row) {
            row.querySelector('td:nth-child(2)').textContent = firstName;
            row.querySelector('td:nth-child(3)').textContent = lastName;
        }
    } else if (collapse.querySelector('.students-container')?.innerHTML.trim()) {
        collapse.dataset.dirty = 'true';
    }
});

connection.on("StudentDeleted", (studentId, groupId) => {
    const collapse = document.getElementById(`collapse-${groupId}`);
    if (!collapse) return;

    if (collapse.classList.contains('show')) {
        const container = collapse.querySelector('.students-container');
        const s = getStudentsSort(collapse);
        fetch(buildStudentsUrl(groupId, s.page, s.key, s.desc))
            .then(r => r.text())
            .then(html => { container.innerHTML = html; });
    } else if (collapse.querySelector('.students-container')?.innerHTML.trim()) {
        collapse.dataset.dirty = 'true';
    }
});

connection.on("GroupAdded", () => searchGroups(currentPage));

connection.on("GroupUpdated", (groupId, name) => {
    const item = document.querySelector(`li[data-id="${groupId}"] strong`);
    if (item) item.textContent = name;
});

connection.on("GroupDeleted", (groupId) => {
    if (document.querySelector(`li[data-id="${groupId}"]`)) searchGroups(currentPage);
});

connection.on("StudentsCleared", (groupId) => {
    const groupItem = document.querySelector(`li[data-id="${groupId}"]`);
    if (groupItem) {
        groupItem.dataset.studentCount = 0;
        const badge = groupItem.querySelector('.badge');
        if (badge) badge.textContent = '0 stu';
    }
    const collapse = document.getElementById(`collapse-${groupId}`);
    if (collapse?.classList.contains('show')) {
        const container = collapse.querySelector('.students-container');
        const s = getStudentsSort(collapse);
        fetch(buildStudentsUrl(groupId, 1, s.key, s.desc))
            .then(r => r.text())
            .then(html => { container.innerHTML = html; });
    }
});

connection.on("StudentAdded", (groupId) => {
    const collapse = document.getElementById(`collapse-${groupId}`);
    if (!collapse) return;

    if (collapse.classList.contains('show')) {
        const container = collapse.querySelector('.students-container');
        const s = getStudentsSort(collapse);
        fetch(buildStudentsUrl(groupId, s.page, s.key, s.desc))
            .then(r => r.text())
            .then(html => { container.innerHTML = html; });
    } else if (collapse.querySelector('.students-container')?.innerHTML.trim()) {
        collapse.dataset.dirty = 'true';
    }
});

let currentPage = 1;
let currentImportGroupId = '';

const _doSearchGroups = debounce(() => {
    const search = document.getElementById('search-input')?.value.trim() ?? '';
    const courseId = document.getElementById('filter-course')?.value ?? '';
    const filter = document.getElementById('filter-students')?.value ?? '0';
    const params = new URLSearchParams({ search, courseId, filter, page: currentPage });
    fetch(`/groups/search?${params}`)
        .then(r => r.text())
        .then(html => { document.querySelector('#groups-accordion').innerHTML = html; });
});

function searchGroups(page = 1) {
    currentPage = page;
    _doSearchGroups();
}

if (document.getElementById('groups-accordion')) {
    document.addEventListener('click', function (e) {

        if (e.target.classList.contains('btn-edit')) {
            const item = e.target.closest('li[data-id]');
            enterEditMode(item);
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
            exitEditMode(item);
        }

        if (e.target.classList.contains('btn-save')) {
            const item = e.target.closest('li[data-id]');
            const id = item.dataset.id;
            const name = item.querySelector('input[name="groupName"]').value;
            const teacherId = item.querySelector('select[name="teacherId"]').value || null;
            apiCall('/groups/edit', 'POST', { id, name, teacherId })
                .then(res => {
                    if (res.success) searchGroups(currentPage);
                    else showError(res.message);
                });
        }

        if (e.target.classList.contains('btn-delete')) {
            const studentRow = e.target.closest('tr[data-id]');

            if (studentRow) {
                const cells = studentRow.querySelectorAll('td');
                const firstName = cells[1].textContent.trim();
                const lastName = cells[2].textContent.trim();
                const modal = new bootstrap.Modal(document.getElementById('deleteStudentModal'));
                document.getElementById('delete-student-name').textContent = `${firstName} ${lastName}`;
                const groupItem = studentRow.closest('li[data-id]');
                document.getElementById('btn-confirm-delete-student').onclick = () => {
                    apiCall(`/students/delete/${studentRow.dataset.id}?groupId=${groupItem.dataset.id}`, 'DELETE')
                        .then(res => {
                            modal.hide();
                            if (res.success) {
                                const newCount = parseInt(groupItem.dataset.studentCount) - 1;
                                groupItem.dataset.studentCount = newCount;
                                const badge = groupItem.querySelector('.badge');
                                if (badge) badge.textContent = `${newCount} stu`;
                            }
                        });
                };
                modal.show();
                return;
            }

            const item = e.target.closest('li[data-id]');
            const studentCount = parseInt(item.dataset.studentCount ?? '0');
            const groupName = item.querySelector('strong').textContent;
            const warning = document.getElementById('delete-group-warning');
            const confirmBtn = document.getElementById('btn-confirm-delete');
            document.getElementById('delete-group-name').textContent = groupName;
            if (studentCount > 0) {
                document.getElementById('delete-group-count').textContent = studentCount;
                warning.classList.remove('d-none');
                confirmBtn.textContent = 'Delete students and group';
            } else {
                warning.classList.add('d-none');
                confirmBtn.textContent = 'Delete';
            }
            const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
            confirmBtn.onclick = () => {
                apiCall(`/groups/delete/${item.dataset.id}?deleteStudents=${studentCount > 0}`, 'DELETE')
                    .then(res => { modal.hide(); if (res.success) searchGroups(currentPage); });
            };
            modal.show();
        }

        if (e.target.classList.contains('btn-clear-students')) {
            const item = e.target.closest('li[data-id]');
            const modal = new bootstrap.Modal(document.getElementById('clearStudentsModal'));
            document.getElementById('btn-confirm-clear').onclick = () => {
                apiCall(`/students/clearstudents?groupId=${item.dataset.id}`, 'POST')
                    .then(res => { if (res.success) { modal.hide(); searchGroups(currentPage); } });
            };
            modal.show();
        }

        if (e.target.classList.contains('btn-page')) {
            const page = parseInt(e.target.dataset.page);
            if (!isNaN(page)) searchGroups(page);
        }

        if (e.target.classList.contains('btn-export-students')) {
            const groupId = e.target.closest('li[data-id]').dataset.id;
            window.location.href = `/students/exportstudents?groupId=${groupId}`;
        }

        if (e.target.classList.contains('btn-import-students')) {
            currentImportGroupId = e.target.closest('li[data-id]').dataset.id;
            document.getElementById('import-students-file').value = '';
            document.getElementById('import-students-result').classList.add('d-none');
            new bootstrap.Modal(document.getElementById('importStudentsModal')).show();
        }
    });
}

document.getElementById('btn-confirm-import-students')?.addEventListener('click', function () {
    const file = document.getElementById('import-students-file')?.files[0];
    const resultDiv = document.getElementById('import-students-result');
    if (!file) return;
    const formData = new FormData();
    formData.append('file', file);
    formData.append('groupId', currentImportGroupId);
    fetch('/students/importstudents', { method: 'POST', body: formData })
        .then(r => r.json())
        .then(res => {
            resultDiv.classList.remove('d-none');
            if (res.success) {
                let html = `<div class="alert alert-success mb-0">Imported: ${res.imported} students</div>`;
                if (res.errors?.length)
                    html += `<ul class="text-danger small mt-2 mb-0">${res.errors.map(e => `<li>${e}</li>`).join('')}</ul>`;
                resultDiv.innerHTML = html;
                document.getElementById('import-students-file').value = '';
                if (res.imported > 0) {
                    const groupItem = document.querySelector(`li[data-id="${currentImportGroupId}"]`);
                    if (groupItem) {
                        const newCount = (parseInt(groupItem.dataset.studentCount) || 0) + res.imported;
                        groupItem.dataset.studentCount = newCount;
                        const badge = groupItem.querySelector('.badge');
                        if (badge) badge.textContent = `${newCount} stu`;
                        const container = groupItem.querySelector('.students-container');
                        if (container) {
                            const collapse = groupItem.querySelector('.accordion-collapse');
                            const s = getStudentsSort(collapse);
                            fetch(buildStudentsUrl(currentImportGroupId, s.page, s.key, s.desc))
                                .then(r => r.text())
                                .then(html => { container.innerHTML = html; });
                        }
                    }
                }
            } else {
                resultDiv.innerHTML = `<div class="alert alert-danger mb-0">${res.message}</div>`;
            }
        });
});

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
        apiCall('/groups/add', 'POST', {
            name: nameInput.value,
            courseId: courseSelect.value,
            teacherId: teacherSelect.value || null
        }).then(res => {
            if (res.success) { resetForm(); searchGroups(currentPage); }
            else showError(res.message);
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
    const input = document.getElementById('search-input');
    const suggestBox = document.getElementById('search-suggestions');
    let suggestAbortController = null;
    let activeIndex = -1;

    function escapeHtml(s) {
        return s.replace(/[&<>"']/g, c => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
        }[c]));
    }

    function highlight(text, query) {
        const safe = escapeHtml(text);
        if (!query) return safe;
        const safeQuery = escapeHtml(query).replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        return safe.replace(new RegExp(`(${safeQuery})`, 'ig'), '<strong>$1</strong>');
    }

    function hideSuggestions() {
        suggestBox.classList.add('d-none');
        suggestBox.innerHTML = '';
        activeIndex = -1;
    }

    function renderSuggestions(items, query) {
        if (!items.length) { hideSuggestions(); return; }
        suggestBox.innerHTML = items.map((it, i) =>
            `<button type="button" class="list-group-item list-group-item-action suggest-item" data-index="${i}" data-value="${escapeHtml(it.name)}">
                ${highlight(it.name, query)}
            </button>`
        ).join('');
        suggestBox.classList.remove('d-none');
        activeIndex = -1;
    }

    function updateActiveHighlight() {
        suggestBox.querySelectorAll('.suggest-item').forEach((el, i) => {
            el.classList.toggle('active', i === activeIndex);
        });
    }

    function selectSuggestion(value) {
        input.value = value;
        hideSuggestions();
        searchGroups(1);
    }

    const _doSuggest = debounce(() => {
        const q = input.value.trim();
        if (q.length < 2) { hideSuggestions(); return; }
        if (suggestAbortController) suggestAbortController.abort();
        suggestAbortController = new AbortController();
        const params = new URLSearchParams({ query: q });
        fetch(`/groups/suggest?${params}`, { signal: suggestAbortController.signal })
            .then(r => r.json())
            .then(items => renderSuggestions(items, q))
            .catch(err => { if (err.name !== 'AbortError') console.error(err); });
    }, 200);

    input.addEventListener('input', () => {
        _doSuggest();
        searchGroups(1);
    });

    input.addEventListener('keydown', (e) => {
        const items = suggestBox.querySelectorAll('.suggest-item');
        if (!items.length) return;
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            activeIndex = (activeIndex + 1) % items.length;
            updateActiveHighlight();
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            activeIndex = (activeIndex - 1 + items.length) % items.length;
            updateActiveHighlight();
        } else if (e.key === 'Enter' && activeIndex >= 0) {
            e.preventDefault();
            selectSuggestion(items[activeIndex].dataset.value);
        } else if (e.key === 'Escape') {
            hideSuggestions();
        }
    });

    suggestBox.addEventListener('mousedown', (e) => {
        const btn = e.target.closest('.suggest-item');
        if (!btn) return;
        e.preventDefault();
        selectSuggestion(btn.dataset.value);
    });

    document.addEventListener('click', (e) => {
        if (!searchForm.contains(e.target)) hideSuggestions();
    });

    document.getElementById('filter-course').addEventListener('change', () => searchGroups(1));
    document.getElementById('filter-students').addEventListener('change', () => searchGroups(1));
}
