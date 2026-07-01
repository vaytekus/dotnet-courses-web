using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Mappers;
using CoursesApp.Web.Models;

namespace CoursesApp.Web.Services
{
    public class GroupService(
        IUnitOfWork uow,
        ILogger<GroupService> logger) : IGroupService
    {
        private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        private readonly ILogger<GroupService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<GroupsIndexViewModel> GetPageAsync(string? search, Guid? courseId, GroupStudentFilter filter, int page, int pageSize)
        {
            _logger.LogInformation("Loading groups page");
            var (groups, total) = await _uow.Groups.GetFilteredGroupAsync(search, courseId, filter, page, pageSize);
            var courses = await _uow.Courses.GetAllCoursesAsync();
            var teachers = await _uow.Teachers.GetAllTeachersAsync();

            return new GroupsIndexViewModel
            {
                Groups = groups.ToGroupDtoList(),
                Courses = courses.ToDtoList(),
                Teachers = teachers.ToDtoList(),
                Page = page,
                TotalCount = total,
                PageSize = pageSize
            };
        }

        public async Task AddGroupAsync(GroupCreateDto dto)
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
            await _uow.SaveAsync();
            _logger.LogInformation("Group {Id} added successfully", group.Id);
        }

        public async Task UpdateGroupAsync(GroupEditDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            _logger.LogInformation("Updating group {Id}", dto.Id);
            var group = await _uow.Groups.GetByIdAsync(dto.Id)
                        ?? throw new KeyNotFoundException($"Group with id {dto.Id} not found");
            group.Name = dto.Name;
            group.TeacherId = dto.TeacherId;
            _uow.Groups.UpdateGroup(group);
            await _uow.SaveAsync();
            _logger.LogInformation("Group {Id} updated successfully", dto.Id);
        }

        public async Task<bool> DeleteGroupAsync(Guid id)
        {
            _logger.LogInformation("Deleting group {Id}", id);
            var group = await _uow.Groups.GetByIdAsync(id)
                        ?? throw new KeyNotFoundException($"Group with id {id} not found");
            if (group.Students.Any())
            {
                _logger.LogWarning("Cannot delete group {Id} — it has {Count} students", id, group.Students.Count);
                return false;
            }
            _uow.Groups.DeleteGroup(group);
            await _uow.SaveAsync();
            _logger.LogInformation("Group {Id} deleted successfully", id);
            return true;
        }
    }
}
