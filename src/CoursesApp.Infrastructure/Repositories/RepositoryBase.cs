namespace CoursesApp.Infrastructure.Repositories;

public abstract class RepositoryBase(AppDbContext context)
{
    protected readonly AppDbContext Context = context ?? throw new ArgumentNullException(nameof(context));
}