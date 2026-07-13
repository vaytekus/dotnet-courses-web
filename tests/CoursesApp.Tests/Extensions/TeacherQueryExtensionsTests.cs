using CoursesApp.Domain.Enums;
using CoursesApp.Infrastructure.Extensions;

namespace CoursesApp.Tests.Extensions;

public class TeacherQueryExtensionsTests
{
    private static IQueryable<Teacher> BuildTeachers() => new List<Teacher>
    {
        new() { Id = Guid.NewGuid(), FirstName = "Charlie", LastName = "Adams" },
        new() { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Clark" },
        new() { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Baker" }
    }.AsQueryable();

    [Fact]
    public void ApplySort_ByFirstNameAscending_OrdersCorrectly()
    {
        var result = BuildTeachers().ApplySort(TeacherSortKey.FirstName, false).ToList();

        Assert.Equal(new[] { "Alice", "Bob", "Charlie" }, result.Select(t => t.FirstName));
    }

    [Fact]
    public void ApplySort_ByFirstNameDescending_OrdersCorrectly()
    {
        var result = BuildTeachers().ApplySort(TeacherSortKey.FirstName, true).ToList();

        Assert.Equal(new[] { "Charlie", "Bob", "Alice" }, result.Select(t => t.FirstName));
    }

    [Fact]
    public void ApplySort_ByLastNameAscending_OrdersCorrectly()
    {
        var result = BuildTeachers().ApplySort(TeacherSortKey.LastName, false).ToList();

        Assert.Equal(new[] { "Adams", "Baker", "Clark" }, result.Select(t => t.LastName));
    }

    [Fact]
    public void ApplySort_ByLastNameDescending_OrdersCorrectly()
    {
        var result = BuildTeachers().ApplySort(TeacherSortKey.LastName, true).ToList();

        Assert.Equal(new[] { "Clark", "Baker", "Adams" }, result.Select(t => t.LastName));
    }
}