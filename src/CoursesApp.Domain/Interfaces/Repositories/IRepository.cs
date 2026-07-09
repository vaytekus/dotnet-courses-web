namespace CoursesApp.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}
