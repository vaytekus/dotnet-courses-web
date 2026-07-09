namespace CoursesApp.Tests.Services;

public class StudentCsvServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IStudentRepository> _students;
    private readonly CsvService _sut;

    public StudentCsvServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _students = new Mock<IStudentRepository>();
        _uow.Setup(u => u.Students).Returns(_students.Object);
        var parsers = new ICsvLineParser[] { new NumberedCsvLineParser(), new SimpleCsvLineParser() };
        _sut = new CsvService(_uow.Object, parsers);
    }

    [Fact]
    public async Task ExportGroupCsvAsync_ReturnsCsvWithNumberedRows()
    {
        var groupId = Guid.NewGuid();
        _students.Setup(r => r.GetStudentsByGroupAsync(groupId)).ReturnsAsync(new List<Student>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith", GroupId = groupId },
            new() { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Jones", GroupId = groupId }
        });

        var bytes = await _sut.ExportGroupCsvAsync(groupId);
        var csv = System.Text.Encoding.UTF8.GetString(bytes);

        Assert.Contains("#,First Name,Last Name", csv);
        Assert.Contains("1,Alice,Smith", csv);
        Assert.Contains("2,Bob,Jones", csv);
    }

    [Fact]
    public async Task ImportGroupCsvAsync_ParsesTwoColumnFormat()
    {
        var groupId = Guid.NewGuid();
        var csv = "First Name,Last Name\nAlice,Smith\nBob,Jones";
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var result = await _sut.ImportGroupCsvAsync(stream, groupId);

        Assert.Equal(2, result.ImportedCount);
        Assert.Empty(result.Errors);
        _students.Verify(r => r.Add(It.IsAny<Student>()), Times.Exactly(2));
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task ImportGroupCsvAsync_ParsesThreeColumnFormat()
    {
        var groupId = Guid.NewGuid();
        var csv = "#,First Name,Last Name\n1,Alice,Smith\n2,Bob,Jones";
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var result = await _sut.ImportGroupCsvAsync(stream, groupId);

        Assert.Equal(2, result.ImportedCount);
        Assert.Empty(result.Errors);
        _students.Verify(r => r.Add(It.Is<Student>(s => s.FirstName == "Alice")), Times.Once);
        _students.Verify(r => r.Add(It.Is<Student>(s => s.FirstName == "Bob")), Times.Once);
    }

    [Fact]
    public async Task ImportGroupCsvAsync_AddsErrorForInvalidLine()
    {
        var groupId = Guid.NewGuid();
        var csv = "First Name,Last Name\nOnlyOneName\nAlice,Smith";
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var result = await _sut.ImportGroupCsvAsync(stream, groupId);

        Assert.Equal(1, result.ImportedCount);
        Assert.Single(result.Errors);
    }
}
