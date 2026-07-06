using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Domain.Interfaces.Repositories;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Mappers;
using CoursesApp.Web.Models;

namespace CoursesApp.Web.Services
{
    public class TeacherService(
        IUnitOfWork uow,
        ILogger<TeacherService> logger) : ITeacherService
    {
        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        private readonly ILogger<TeacherService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<TeachersIndexViewModel> GetPageAsync(string? search, int page, int pageSize, CancellationToken ct = default)
        {
            var (teachers, total) = await _uow.Teachers.GetFilteredTeachersAsync(search, page, pageSize, ct);
            return new TeachersIndexViewModel
            {
                Teachers = teachers.ToDtoList(),
                Page = page,
                TotalCount = total,
                PageSize = pageSize
            };
        }

        public async Task AddTeacherAsync(TeacherDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            _logger.LogInformation("Adding teacher {FirstName} {LastName}", dto.FirstName, dto.LastName);
            var teacher = new Domain.Entities.Teacher
            {
                Id = Guid.NewGuid(),
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };
            _uow.Teachers.AddTeacher(teacher);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("Teacher {Id} added successfully", teacher.Id);
        }

        public async Task UpdateTeacherAsync(TeacherEditDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            _logger.LogInformation("Updating teacher {Id}", dto.Id);
            var teacher = await _uow.Teachers.GetTeacherByIdAsync(dto.Id, ct);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher {Id} not found", dto.Id);
                throw new KeyNotFoundException($"Teacher {dto.Id} not found");
            }
            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            _uow.Teachers.UpdateTeacher(teacher);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("Teacher {Id} updated successfully", dto.Id);
        }

        public async Task ValidateExistAsync(Guid id, CancellationToken ct = default)
        {
            var exists = await _uow.Teachers.GetTeacherByIdAsync(id, ct);
            if (exists is null)
            {
                throw new KeyNotFoundException($"Teacher {id} not found");
            }
        }

        public async Task DeleteTeacherAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Deleting teacher {Id}", id);
            var teacher = await _uow.Teachers.GetTeacherByIdAsync(id, ct);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher {Id} not found", id);
                throw new KeyNotFoundException("Teacher not found");
            }

            _uow.Teachers.DeleteTeacher(teacher);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("Teacher {Id} deleted successfully", id);
        }
    }
}
