namespace CoursesApp.Tests.Services;

public class TeacherServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<ITeacherRepository> _teachers;
    private readonly Mock<IGroupRepository> _groups;
    private readonly TeacherService _sut;

    public TeacherServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _teachers = new Mock<ITeacherRepository>();
        _groups = new Mock<IGroupRepository>();
        var logger = new Mock<ILogger<TeacherService>>();
        _uow.Setup(u => u.Teachers).Returns(_teachers.Object);
        _uow.Setup(u => u.Groups).Returns(_groups.Object);
        _sut = new TeacherService(_uow.Object, logger.Object);
    }

    [Fact]
    public async Task AddTeacherAsync_ThrowsArgumentNullException_WhenDtoIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddTeacherAsync(null!));
    }

    [Fact]
    public async Task AddTeacherAsync_CallsAddTeacherAndSave_WhenDtoIsValid()
    {
        var dto = new TeacherDto(null, "Jane", "Doe");

        await _sut.AddTeacherAsync(dto);

        _teachers.Verify(r => r.AddTeacher(It.Is<Teacher>(t =>
            t.FirstName == dto.FirstName &&
            t.LastName == dto.LastName)), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTeacherAsync_ThrowsKeyNotFoundException_WhenTeacherNotFound()
    {
        _teachers.Setup(r => r.GetTeacherByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Teacher?)null);
        var dto = new TeacherEditDto(Guid.NewGuid(), "A", "B");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateTeacherAsync(dto));
    }

    [Fact]
    public async Task UpdateTeacherAsync_UpdatesFieldsAndSaves_WhenTeacherExists()
    {
        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "Old", LastName = "Name" };
        _teachers.Setup(r => r.GetTeacherByIdAsync(teacher.Id)).ReturnsAsync(teacher);
        var dto = new TeacherEditDto(teacher.Id, "New", "Name");

        await _sut.UpdateTeacherAsync(dto);

        Assert.Equal("New", teacher.FirstName);
        _teachers.Verify(r => r.UpdateTeacher(teacher), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task ValidateExistAsync_ThrowsKeyNotFoundException_WhenTeacherNotFound()
    {
        _teachers.Setup(r => r.GetTeacherByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Teacher?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ValidateExistAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ValidateExistAsync_DoesNotThrow_WhenTeacherExists()
    {
        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "A", LastName = "B" };
        _teachers.Setup(r => r.GetTeacherByIdAsync(teacher.Id)).ReturnsAsync(teacher);

        var ex = await Record.ExceptionAsync(() => _sut.ValidateExistAsync(teacher.Id));

        Assert.Null(ex);
    }

    [Fact]
    public async Task DeleteTeacherAsync_ThrowsKeyNotFoundException_WhenTeacherNotFound()
    {
        _teachers.Setup(r => r.GetTeacherByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Teacher?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteTeacherAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteTeacherAsync_DeletesAndSaves_WhenTeacherExists()
    {
        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "A", LastName = "B" };
        _teachers.Setup(r => r.GetTeacherByIdAsync(teacher.Id)).ReturnsAsync(teacher);

        await _sut.DeleteTeacherAsync(teacher.Id);

        _teachers.Verify(r => r.DeleteTeacher(teacher), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }
}
