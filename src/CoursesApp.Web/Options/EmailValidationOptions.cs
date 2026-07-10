namespace CoursesApp.Web.Options;

public class EmailValidationOptions
{
    public IReadOnlyList<string> BlacklistedDomains { get; init; } = [];
}
