namespace CoursesApp.Web.Services;

public interface IEmailDomainValidator
{
    Task<bool> HasMxRecordAsync(string email, CancellationToken ct = default);
}
