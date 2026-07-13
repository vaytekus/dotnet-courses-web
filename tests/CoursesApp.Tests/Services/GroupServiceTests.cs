using CoursesApp.Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;

namespace CoursesApp.Tests.Services;

public class GroupServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IGroupRepository> _groups;
    private readonly GroupService _sut;

    public GroupServiceTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _groups = new Mock<IGroupRepository>();
        var logger = new Mock<ILogger<GroupService>>();
        var cache = new Mock<IMemoryCache>();
        _uow.Setup(u => u.Groups).Returns(_groups.Object);
        _sut = new GroupService(_uow.Object, cache.Object, logger.Object);
    }

    [Fact]
    public async Task AddGroupAsync_ThrowsArgumentNullException_WhenDtoIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddGroupAsync(null!));
    }

    [Fact]
    public async Task AddGroupAsync_CallsAddGroupAndSave_WhenDtoIsValid()
    {
        var dto = new GroupCreateDto("Group A", Guid.NewGuid(), null);

        await _sut.AddGroupAsync(dto);

        _groups.Verify(r => r.Add(It.Is<Group>(g =>
            g.Name == dto.Name &&
            g.CourseId == dto.CourseId)), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateGroupAsync_ThrowsKeyNotFoundException_WhenGroupNotFound()
    {
        _groups.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Group?)null);
        var dto = new GroupEditDto(Guid.NewGuid(), "New Name", null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateGroupAsync(dto));
    }

    [Fact]
    public async Task UpdateGroupAsync_UpdatesNameAndSaves_WhenGroupExists()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Old Name", CourseId = Guid.NewGuid() };
        _groups.Setup(r => r.GetByIdAsync(group.Id)).ReturnsAsync(group);
        var dto = new GroupEditDto(group.Id, "New Name", null);

        await _sut.UpdateGroupAsync(dto);

        Assert.Equal("New Name", group.Name);
        _groups.Verify(r => r.Update(group), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllSelectAsync_ReturnsAllGroupsAsDtos()
    {
        var groups = new List<Group>
        {
            new() { Id = Guid.NewGuid(), Name = "Group A", CourseId = Guid.NewGuid() },
            new() { Id = Guid.NewGuid(), Name = "Group B", CourseId = Guid.NewGuid() }
        };
        _groups.Setup(r => r.GetAllGroupAsync(It.IsAny<CancellationToken>())).ReturnsAsync(groups);

        var result = await _sut.GetAllSelectAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Name == "Group A");
        Assert.Contains(result, g => g.Name == "Group B");
    }

    [Fact]
    public async Task DeleteGroupAsync_ThrowsKeyNotFoundException_WhenGroupNotFound()
    {
        _groups.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Group?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteGroupAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteGroupAsync_ReturnsFalse_WhenGroupHasStudents()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "G", CourseId = Guid.NewGuid() };
        group.Students.Add(new Student { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", GroupId = group.Id });
        _groups.Setup(r => r.GetByIdAsync(group.Id)).ReturnsAsync(group);

        var result = await _sut.DeleteGroupAsync(group.Id);

        Assert.False(result);
        _groups.Verify(r => r.Delete(It.IsAny<Group>()), Times.Never);
    }

    [Fact]
    public async Task DeleteGroupAsync_DeletesAndReturnsTrue_WhenGroupIsEmpty()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "G", CourseId = Guid.NewGuid() };
        _groups.Setup(r => r.GetByIdAsync(group.Id)).ReturnsAsync(group);

        var result = await _sut.DeleteGroupAsync(group.Id);

        Assert.True(result);
        _groups.Verify(r => r.Delete(group), Times.Once);
        _uow.Verify(u => u.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task AddGroupAsync_ThrowsDuplicateNameException_WhenNameExists()
    {
        _groups.Setup(r => r.NameExistsAsync("Group A", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var dto = new GroupCreateDto("Group A", Guid.NewGuid(), null);

        await Assert.ThrowsAsync<DuplicateNameException>(() => _sut.AddGroupAsync(dto));
        _groups.Verify(r => r.Add(It.IsAny<Group>()), Times.Never);
        _uow.Verify(u => u.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task AddGroupAsync_TrimsNameBeforeCheckAndSave()
    {
        _groups.Setup(r => r.NameExistsAsync("Group A", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var dto = new GroupCreateDto("  Group A  ", Guid.NewGuid(), null);

        await _sut.AddGroupAsync(dto);

        _groups.Verify(r => r.NameExistsAsync("Group A", null, It.IsAny<CancellationToken>()), Times.Once);
        _groups.Verify(r => r.Add(It.Is<Group>(g => g.Name == "Group A")), Times.Once);
    }

    [Fact]
    public async Task UpdateGroupAsync_ThrowsDuplicateNameException_WhenNameExists()
    {
        var id = Guid.NewGuid();
        _groups.Setup(r => r.NameExistsAsync("Duplicate", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var dto = new GroupEditDto(id, "Duplicate", null);

        await Assert.ThrowsAsync<DuplicateNameException>(() => _sut.UpdateGroupAsync(dto));
        _groups.Verify(r => r.Update(It.IsAny<Group>()), Times.Never);
        _uow.Verify(u => u.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateGroupAsync_PassesExcludeId_WhenCheckingDuplicate()
    {
        var group = new Group { Id = Guid.NewGuid(), Name = "Old", CourseId = Guid.NewGuid() };
        _groups.Setup(r => r.NameExistsAsync("New", group.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _groups.Setup(r => r.GetByIdAsync(group.Id)).ReturnsAsync(group);
        var dto = new GroupEditDto(group.Id, "  New  ", null);

        await _sut.UpdateGroupAsync(dto);

        _groups.Verify(r => r.NameExistsAsync("New", group.Id, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("New", group.Name);
    }
}
