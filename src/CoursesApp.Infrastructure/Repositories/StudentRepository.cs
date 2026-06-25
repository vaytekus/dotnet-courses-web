using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoursesApp.Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _context.Students.ToListAsync();
        }

        public async Task<List<Student>> GetStudentsByGroupAsync(Guid groupId)
        {
            return await _context.Students
                .Where(s => s.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(Guid id)
        {
            return await _context.Students.FindAsync(id);
        }

        public void UpdateStudent(Student student)
        {
            _context.Students.Update(student);
        }

        public void DeleteStudent(Student student)
        {
            _context.Students.Remove(student);
        }
    }
}