using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public class TeacherService(IUnitOfWork uow) : ITeacherService
    {
        public async Task UpdateTeacherAsync(TeacherEditDto dto)
        {
            var teacher  = await uow.Teachers.GetTeacherByIdAsync(dto.Id);
            if (teacher == null)
            {
                throw new KeyNotFoundException($"Teacher {dto.Id} not found"); 
            }
            
            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;

            uow.Teachers.UpdateTeacher(teacher);
            await uow.SaveAsync();
        }

        public async Task DeleteTeacherAsync(Guid id)
        {
            var teacher = await uow.Teachers.GetTeacherByIdAsync(id);
            if (teacher == null)
            {
                throw new KeyNotFoundException("Teacher not found"); 
            }

            uow.Teachers.DeleteTeacher(teacher);
            await uow.SaveAsync();
        }
    }
}