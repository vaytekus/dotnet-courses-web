using System.Text;
using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
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

        public async Task<StudentsIndexViewModel> GetPageAsync(string? search, Guid? id, int page, int pageSize)
        {
            _logger.LogInformation("Getting page {Page} of {PageSize}", page, pageSize);
            var (students, total) = await _uow.Students.GetFilteredStudentAsync(search, id, page, pageSize);
            var groups = await _uow.Groups.GetAllGroupAsync();

            return new StudentsIndexViewModel
            {
                Students = students.ToDtoList(),
                Groups = groups.ToSelectDtoList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<List<StudentDto>> GetStudentsByGroupAsync(Guid groupId)
        {
            _logger.LogInformation("Loading students for group {GroupId}", groupId);
            var students = await _uow.Students.GetStudentsByGroupAsync(groupId);
            return students.ToDtoList();
        }

        public async Task AddStudentAsync(StudentDto studentDto)
        {
            ArgumentNullException.ThrowIfNull(studentDto);
            _logger.LogInformation("Adding student {FirstName} {LastName} to group {GroupId}",
                studentDto.FirstName, studentDto.LastName, studentDto.GroupId);
            var student = new Student
            {
                Id = Guid.NewGuid(),
                FirstName = studentDto.FirstName,
                LastName = studentDto.LastName,
                GroupId = studentDto.GroupId ?? Guid.Empty
            };
            _uow.Students.AddStudent(student);
            await _uow.SaveAsync();
            _logger.LogInformation("Student {Id} added successfully", student.Id);
        }

        public async Task UpdateStudentAsync(StudentEditDto studentDto)
        {
            ArgumentNullException.ThrowIfNull(studentDto);
            _logger.LogInformation("Updating student {Id}", studentDto.Id);
            var student = await _uow.Students.GetStudentByIdAsync(studentDto.Id);
            if (student == null)
            {
                _logger.LogWarning("Student {Id} not found", studentDto.Id);
                throw new KeyNotFoundException($"Student {studentDto.Id} not found");
            }
            student.FirstName = studentDto.FirstName;
            student.LastName = studentDto.LastName;
            student.GroupId = studentDto.GroupId;
            _uow.Students.UpdateStudent(student);
            await _uow.SaveAsync();
            _logger.LogInformation("Student {Id} updated successfully", studentDto.Id);
        }

        public async Task DeleteStudentAsync(Guid id)
        {
            _logger.LogInformation("Deleting student {Id}", id);
            var student = await _uow.Students.GetStudentByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Student {Id} not found", id);
                throw new KeyNotFoundException("Student not found");
            }
            _uow.Students.DeleteStudent(student);
            await _uow.SaveAsync();
            _logger.LogInformation("Student {Id} deleted successfully", id);
        }

        public async Task ClearAllStudentsAsync(Guid id)
        {
            _logger.LogInformation("Clearing all students from group {GroupId}", id);
            await _uow.Students.DeleteAllByGroupAsync(id);
            await _uow.SaveAsync();
            _logger.LogInformation("All students cleared from group {GroupId}", id);
        }

        public async Task<byte[]> ExportGroupCsvAsync(Guid groupId)
        {
            var students = await _uow.Students.GetStudentsByGroupAsync(groupId);
            var sb = new StringBuilder();
            sb.AppendLine("#,First Name,Last Name");

            var i = 1;
            foreach (var student in students)
            {
                sb.AppendLine($"{i++},{Escape(student.FirstName)},{Escape(student.LastName)}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<ImportResult> ImportGroupCsvAsync(Stream stream, Guid groupId)
        {
            var result = new ImportResult();
            using var reader = new StreamReader(stream);
            await reader.ReadLineAsync();
            var lineNumber = 1;
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(',');

                string firstName;
                string lastName;

                if (parts.Length >= 3 && int.TryParse(parts[0].Trim(), out _))
                {
                    firstName = parts[1].Trim().Trim('"');
                    lastName = parts[2].Trim().Trim('"');
                }
                else if (parts.Length >= 2)
                {
                    firstName = parts[0].Trim().Trim('"');
                    lastName = parts[1].Trim().Trim('"');
                }
                else
                {
                    result.Errors.Add($"Line {lineNumber} is invalid");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    result.Errors.Add($"Line {lineNumber}: first and last name are required!");
                    continue;
                }

                _uow.Students.AddStudent(new Student
                {
                    Id = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = lastName,
                    GroupId = groupId
                });
                
                result.ImportedCount++;
            }

            if (result.ImportedCount > 0)
            {
                await _uow.SaveAsync();
            }
            
            return result;
        }

        private static string Escape(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            
            return value;
        }
    }
}
