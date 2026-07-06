using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces.Repositories;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoursesApp.Tests.Services
{
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
            var dto = new GroupCreateDto { Name = "Group A", CourseId = Guid.NewGuid(), TeacherId = null };

            await _sut.AddGroupAsync(dto);

            _groups.Verify(r => r.AddGroup(It.Is<Group>(g =>
                g.Name == dto.Name &&
                g.CourseId == dto.CourseId)), Times.Once);
            _uow.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateGroupAsync_ThrowsKeyNotFoundException_WhenGroupNotFound()
        {
            _groups.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Group?)null);
            var dto = new GroupEditDto { Id = Guid.NewGuid(), Name = "New Name" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateGroupAsync(dto));
        }

        [Fact]
        public async Task UpdateGroupAsync_UpdatesNameAndSaves_WhenGroupExists()
        {
            var group = new Group { Id = Guid.NewGuid(), Name = "Old Name", CourseId = Guid.NewGuid() };
            _groups.Setup(r => r.GetByIdAsync(group.Id)).ReturnsAsync(group);
            var dto = new GroupEditDto { Id = group.Id, Name = "New Name", TeacherId = null };

            await _sut.UpdateGroupAsync(dto);

            Assert.Equal("New Name", group.Name);
            _groups.Verify(r => r.UpdateGroup(group), Times.Once);
            _uow.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteGroupAsync_ReturnsFalse_WhenGroupHasStudents()
        {
            var group = new Group { Id = Guid.NewGuid(), Name = "G", CourseId = Guid.NewGuid() };
            group.Students.Add(new Student { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", GroupId = group.Id });
            _groups.Setup(r => r.GetByIdAsync(group.Id)).ReturnsAsync(group);

            var result = await _sut.DeleteGroupAsync(group.Id);

            Assert.False(result);
            _groups.Verify(r => r.DeleteGroup(It.IsAny<Group>()), Times.Never);
        }

        [Fact]
        public async Task DeleteGroupAsync_DeletesAndReturnsTrue_WhenGroupIsEmpty()
        {
            var group = new Group { Id = Guid.NewGuid(), Name = "G", CourseId = Guid.NewGuid() };
            _groups.Setup(r => r.GetByIdAsync(group.Id)).ReturnsAsync(group);

            var result = await _sut.DeleteGroupAsync(group.Id);

            Assert.True(result);
            _groups.Verify(r => r.DeleteGroup(group), Times.Once);
            _uow.Verify(u => u.SaveAsync(), Times.Once);
        }
    }
}