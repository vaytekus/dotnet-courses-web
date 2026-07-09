namespace CoursesApp.Tests.Services;

public class StudentServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IStudentRepository> _students;
    private readonly StudentService _sut;

    public StudentServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _students = new Mock<IStudentRepository>();
        var logger = new Mock<ILogger<StudentService>>();
        _uow.Setup(u => u.Students).Returns(_students.Object);
        _sut = new StudentService(_uow.Object, logger.Object);
    }

    [Fact]
    public async Task AddStudentAsync_ThrowsArgumentNullException_WhenDtoIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddStudentAsync(null!));
    }

    [Fact]
    public async Task AddStudentAsync_CallsAddStudentAndSave_WhenDtoIsValid()
    {
        var dto = new StudentDto(null, "John", "Doe", Guid.NewGuid());

        await _sut.AddStudentAsync(dto);

        _students.Verify(r => r.Add(It.Is<Student>(s =>
            s.FirstName == dto.FirstName &&
            s.LastName == dto.LastName)), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStudentAsync_ThrowsKeyNotFoundException_WhenStudentNotFound()
    {
        _students.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Student?)null);
        var dto = new StudentEditDto(Guid.NewGuid(), "A", "B", Guid.NewGuid());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateStudentAsync(dto));
    }

    [Fact]
    public async Task UpdateStudentAsync_UpdatesFieldsAndSaves_WhenStudentExists()
    {
        var student = new Student { Id = Guid.NewGuid(), FirstName = "Old", LastName = "Name", GroupId = Guid.NewGuid() };
        _students.Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>())).ReturnsAsync(student);
        var dto = new StudentEditDto(student.Id, "New", "Name", student.GroupId);

        await _sut.UpdateStudentAsync(dto);

        Assert.Equal("New", student.FirstName);
        _students.Verify(r => r.Update(student), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsync_ThrowsKeyNotFoundException_WhenStudentNotFound()
    {
        _students.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Student?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteStudentAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteStudentAsync_DeletesAndSaves_WhenStudentExists()
    {
        var student = new Student { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", GroupId = Guid.NewGuid() };
        _students.Setup(r => r.GetByIdAsync(student.Id, It.IsAny<CancellationToken>())).ReturnsAsync(student);

        await _sut.DeleteStudentAsync(student.Id);

        _students.Verify(r => r.Delete(student), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task ClearAllStudentsAsync_CallsDeleteAllAndSave()
    {
        var groupId = Guid.NewGuid();
        _students.Setup(r => r.DeleteAllByGroupAsync(groupId)).Returns(Task.CompletedTask);

        await _sut.ClearAllStudentsAsync(groupId);

        _students.Verify(r => r.DeleteAllByGroupAsync(groupId), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

}
