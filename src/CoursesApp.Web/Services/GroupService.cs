using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public class GroupService(IUnitOfWork uow) : IGroupService
    {
        public async Task AddGroupAsync(GroupCreateDto dto)
        {
            var group = new Group
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CourseId = dto.CourseId,
                TeacherId = dto.TeacherId,
            };

            uow.Groups.AddGroup(group);
            await uow.SaveAsync();
        }

        public async Task UpdateGroupAsync(GroupEditDto dto)
        {
            var group = await uow.Groups.GetByIdAsync(dto.Id) 
                        ?? throw new KeyNotFoundException($"Group with id {dto.Id} not found");
            
            group.Name = dto.Name;
            group.TeacherId = dto.TeacherId;
            
            uow.Groups.UpdateGroup(group);
            await uow.SaveAsync();
        }

        public async Task<bool> DeleteGroupAsync(Guid id)
        {
            var group = await uow.Groups.GetByIdAsync(id) 
                        ?? throw new KeyNotFoundException($"Group with id {id} not found");

            if (group.Students.Any())
            {
                return false;
            }
            
            uow.Groups.DeleteGroup(group);
            await uow.SaveAsync();
            return true;
        }

        public async Task ClearStudentAsync(Guid id)
        {
            var group = await uow.Groups.GetByIdAsync(id) 
                        ?? throw new KeyNotFoundException($"Group with id {id} not found");
            
            group.Students.Clear();
            await uow.SaveAsync();
        }
    }
}