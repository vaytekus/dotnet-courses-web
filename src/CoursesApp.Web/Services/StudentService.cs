using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public class StudentService(IUnitOfWork uow) : IStudentService
    {
        public async Task UpdateStudentAsync(StudentEditDto dto)
        {
            var student  = await uow.Students.GetStudentByIdAsync(dto.Id);
            if (student == null)
            {
                throw new KeyNotFoundException($"Student {dto.Id} not found"); 
            }
            
            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.GroupId = dto.GroupId;

            uow.Students.UpdateStudent(student);
            await uow.SaveAsync();
        }

        public async Task DeleteStudentAsync(Guid id)
        {
            var student = await uow.Students.GetStudentByIdAsync(id);
            if (student == null)
            {
                throw new KeyNotFoundException("Student not found"); 
            }

            uow.Students.DeleteStudent(student);
            await uow.SaveAsync();
        }
    }
}