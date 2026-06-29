using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public class StudentService(IUnitOfWork uow) : IStudentService
    {
        public async Task AddStudentAsync(StudentDto studentDto)
        {
            var student = new Student
            {
                Id = Guid.NewGuid(),
                FirstName = studentDto.FirstName,
                LastName = studentDto.LastName,
                GroupId = studentDto.GroupId ?? Guid.Empty
            };
            
            uow.Students.AddStudent(student);
            await uow.SaveAsync();
        }
        
        public async Task UpdateStudentAsync(StudentEditDto studentDto)
        {
            var student  = await uow.Students.GetStudentByIdAsync(studentDto.Id);
            if (student == null)
            {
                throw new KeyNotFoundException($"Student {studentDto.Id} not found"); 
            }
            
            student.FirstName = studentDto.FirstName;
            student.LastName = studentDto.LastName;
            student.GroupId = studentDto.GroupId;

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