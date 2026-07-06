using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;
using CoursesApp.Domain.Interfaces.Repositories;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Mappers;
using CoursesApp.Web.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CoursesApp.Web.Services
{
    public class GroupService(
        IUnitOfWork uow,
        IMemoryCache cache,
        ILogger<GroupService> logger) : IGroupService
    {
        private static readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);
        private const string _coursesCacheKey = "courses_all";
        private const string _teachersCacheKey = "teachers_all";

        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        private readonly ILogger<GroupService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        
        public async Task<GroupsIndexViewModel> GetPageAsync(
            string? search, 
            Guid? courseId, 
            GroupStudentFilter filter, 
            int page, 
            int pageSize, 
            CancellationToken ct = default)
        {
            _logger.LogInformation("Loading groups page");
            var (groups, total) = await _uow.Groups.GetFilteredGroupAsync(search, courseId, filter, page, pageSize, ct);
            if (!_cache.TryGetValue(_coursesCacheKey, out List<Course>? courses))
            {
                courses = await _uow.Courses.GetAllCoursesAsync(ct);
                _cache.Set(_coursesCacheKey, courses, _cacheTtl);
            }

            if (!_cache.TryGetValue(_teachersCacheKey, out List<Teacher>? teachers))
            {
                teachers = await _uow.Teachers.GetAllTeachersAsync(ct);
                _cache.Set(_teachersCacheKey, teachers, _cacheTtl);
            }

            return new GroupsIndexViewModel
            {
                Groups = groups.ToGroupDtoList(),
                Courses = courses!.ToDtoList(),
                Teachers = teachers!.ToDtoList(),
                Page = page,
                TotalCount = total,
                PageSize = pageSize
            };
        }

        public async Task AddGroupAsync(GroupCreateDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            _logger.LogInformation("Adding group {Name} for course {CourseId}", dto.Name, dto.CourseId);
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CourseId = dto.CourseId,
                TeacherId = dto.TeacherId,
            };
            _uow.Groups.AddGroup(group);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("Group {Id} added successfully", group.Id);
            _cache.Remove(_coursesCacheKey);
            _cache.Remove(_teachersCacheKey);
        }

        public async Task UpdateGroupAsync(GroupEditDto dto, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(dto);
            _logger.LogInformation("Updating group {Id}", dto.Id);
            var group = await _uow.Groups.GetByIdAsync(dto.Id, ct)
                        ?? throw new KeyNotFoundException($"Group with id {dto.Id} not found");
            group.Name = dto.Name;
            group.TeacherId = dto.TeacherId;
            _uow.Groups.UpdateGroup(group);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("Group {Id} updated successfully", dto.Id);
            _cache.Remove(_coursesCacheKey);
            _cache.Remove(_teachersCacheKey);
        }

        public async Task UnassignTeacherAsync(Guid teacherId, CancellationToken ct = default)
        {
            await _uow.Groups.UnassignTeacherAsync(teacherId, ct);
        }

        public async Task<bool> DeleteGroupAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Deleting group {Id}", id);
            var group = await _uow.Groups.GetByIdAsync(id, ct)
                        ?? throw new KeyNotFoundException($"Group with id {id} not found");
            if (group.Students.Any())
            {
                _logger.LogWarning("Cannot delete group {Id} — it has {Count} students", id, group.Students.Count);
                return false;
            }
            _uow.Groups.DeleteGroup(group);
            await _uow.SaveAsync(ct);
            _logger.LogInformation("Group {Id} deleted successfully", id);
            _cache.Remove(_coursesCacheKey);
            _cache.Remove(_teachersCacheKey);
            return true;
        }
    }
}
