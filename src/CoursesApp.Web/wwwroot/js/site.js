// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function(){
    document.querySelectorAll('.accordion-collapse[data-group-id]').forEach(el => {
        el.addEventListener('show.bs.collapse', function(){
            const groupId = this.dataset.groupId;
            const container = this.querySelector('.students-container');
            if(container.innerHTML.trim()) return;
            
            fetch(`/Students/GetStudentsByGroupId?groupId=${groupId}`)
                .then(res => res.text())
                .then(html => container.innerHTML = html);
        })
    });
});
