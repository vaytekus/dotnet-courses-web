using CoursesApp.Domain.Interfaces;

namespace CoursesApp.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        
        public ICourseRepository Courses { get; }
        public IGroupRepository Groups { get; }
        public ITeacherRepository Teachers { get; }
        public IStudentRepository Students { get; }
        
        public UnitOfWork(
            AppDbContext context,
            ICourseRepository courses,
            IGroupRepository groups,
            ITeacherRepository teachers,
            IStudentRepository students)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(courses);
            ArgumentNullException.ThrowIfNull(groups);
            ArgumentNullException.ThrowIfNull(teachers);
            ArgumentNullException.ThrowIfNull(students);
            _context = context;
            Courses = courses;
            Groups = groups;
            Teachers = teachers;
            Students = students;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}