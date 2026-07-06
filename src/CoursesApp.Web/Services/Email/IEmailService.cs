namespace CoursesApp.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlBody, CancellationToken ct = default);
    }
}