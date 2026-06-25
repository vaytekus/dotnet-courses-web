if(document.getElementById('students-table')){
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('btn-edit')) {
            const row = e.target.closest('tr');
            row.querySelectorAll('.view-mode').forEach(el => el.classList.add('d-none'));
            row.querySelectorAll('.edit-mode').forEach(el => el.classList.remove('d-none'));
        }
        
        // Cancel
        if (e.target.classList.contains('btn-cancel')) {
            const row = e.target.closest('tr');
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
            const id = row.dataset.id;
            if (confirm('Are you sure you want to delete this student?')) {
                fetch(`/students/delete/${id}`, { method: 'DELETE' })
                    .then(r => r.json())
                    .then(res => { if (res.success) row.remove(); });
            }
        }
    });
}