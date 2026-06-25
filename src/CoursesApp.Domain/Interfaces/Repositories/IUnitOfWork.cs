namespace CoursesApp.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        IGroupRepository Groups { get; }
        ITeacherRepository Teachers { get; }
        IStudentRepository Students { get; }
        Task SaveAsync();
    }
}