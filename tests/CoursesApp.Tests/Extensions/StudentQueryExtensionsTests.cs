using CoursesApp.Domain.Enums;
using CoursesApp.Infrastructure.Extensions;

namespace CoursesApp.Tests.Extensions;

public class StudentQueryExtensionsTests
{
    private static IQueryable<Student> BuildStudents() => new List<Student>
    {
        new() { Id = Guid.NewGuid(), FirstName = "Charlie", LastName = "Adams", GroupId = Guid.NewGuid() },
        new() { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Clark", GroupId = Guid.NewGuid() },
        new() { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Baker", GroupId = Guid.NewGuid() }
    }.AsQueryable();

    [Fact]
    public void ApplySort_ByFirstNameAscending_OrdersCorrectly()
    {
        var result = BuildStudents().ApplySort(StudentSortKey.FirstName, false).ToList();

        Assert.Equal(new[] { "Alice", "Bob", "Charlie" }, result.Select(s => s.FirstName));
    }

    [Fact]
    public void ApplySort_ByFirstNameDescending_OrdersCorrectly()
    {
        var result = BuildStudents().ApplySort(StudentSortKey.FirstName, true).ToList();

        Assert.Equal(new[] { "Charlie", "Bob", "Alice" }, result.Select(s => s.FirstName));
    }

    [Fact]
    public void ApplySort_ByLastNameAscending_OrdersCorrectly()
    {
        var result = BuildStudents().ApplySort(StudentSortKey.LastName, false).ToList();

        Assert.Equal(new[] { "Adams", "Baker", "Clark" }, result.Select(s => s.LastName));
    }

    [Fact]
    public void ApplySort_ByLastNameDescending_OrdersCorrectly()
    {
        var result = BuildStudents().ApplySort(StudentSortKey.LastName, true).ToList();

        Assert.Equal(new[] { "Clark", "Baker", "Adams" }, result.Select(s => s.LastName));
    }
}