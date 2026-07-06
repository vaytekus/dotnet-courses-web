namespace CoursesApp.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        IGroupRepository Groups { get; }
        ITeacherRepository Teachers { get; }
        IStudentRepository Students { get; }
        Task SaveAsync(CancellationToken ct = default);
    }
}