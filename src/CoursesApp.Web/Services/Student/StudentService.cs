using CoursesApp.Domain.Interfaces.Repositories;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Mappers;
using CoursesApp.Web.Models;

namespace CoursesApp.Web.Services
{
    public class StudentService(
        IUnitOfWork uow,
        ILogger<StudentService> logger) : IStudentService
    {
        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        private readonly ILogger<StudentService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<StudentsIndexViewModel> GetPageAsync(string? search, Guid? id, int page, int pageSize, CancellationToken ct = default)
        {
            _logger.LogInformation("Getting page {Page} of {PageSize}", page, pageSize);
            var (students, total) = await _uow.Students.GetFilteredStudentAsync(search, id, page, pageSize, ct);
            var groups = await _uow.Groups.GetAllGroupAsync(ct);

            return new StudentsIndexViewModel
            {
                Students = students.ToDtoList(),
                Groups = groups.ToSelectDtoList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<List<StudentDto>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default)
        {
            _logger.LogInformation("Loading students for group {GroupId}", groupId);
            var students = await _uow.Students.GetStudentsByGroupAsync(groupId, ct);
            return students.ToDtoList();
        }

        public async Task AddStudentAsync(StudentDto studentDto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(studentDto);
            _logger.LogInformation("Adding student {FirstName} {LastName} to group {GroupId}",
                studentDto.FirstName, studentDto.LastName, studentDto.GroupId);
            var student = new Domain.Entities.Student
            {
                Id = Guid.NewGuid(),
                FirstName = studentDto.FirstName,
                LastName = studentDto.LastName,
                GroupId = studentDto.GroupId ?? Guid.Empty
            };
            _uow.Students.AddStudent(student);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("Student {Id} added successfully", student.Id);
        }

        public async Task UpdateStudentAsync(StudentEditDto studentDto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(studentDto);
            _logger.LogInformation("Updating student {Id}", studentDto.Id);
            var student = await _uow.Students.GetStudentByIdAsync(studentDto.Id, ct);
            if (student == null)
            {
                _logger.LogWarning("Student {Id} not found", studentDto.Id);
                throw new KeyNotFoundException($"Student {studentDto.Id} not found");
            }
            student.FirstName = studentDto.FirstName;
            student.LastName = studentDto.LastName;
            student.GroupId = studentDto.GroupId;
            _uow.Students.UpdateStudent(student);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("Student {Id} updated successfully", studentDto.Id);
        }

        public async Task DeleteStudentAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Deleting student {Id}", id);
            var student = await _uow.Students.GetStudentByIdAsync(id, ct);
            if (student == null)
            {
                _logger.LogWarning("Student {Id} not found", id);
                throw new KeyNotFoundException("Student not found");
            }
            _uow.Students.DeleteStudent(student);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("Student {Id} deleted successfully", id);
        }

        public async Task ClearAllStudentsAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Clearing all students from group {GroupId}", id);
            await _uow.Students.DeleteAllByGroupAsync(id, ct);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("All students cleared from group {GroupId}", id);
        }

        
    }
}
