using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Domain.Interfaces.Repositories;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CoursesApp.Tests.Services
{
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
            var dto = new StudentDto { FirstName = "John", LastName = "Doe", GroupId = Guid.NewGuid() };

            await _sut.AddStudentAsync(dto);

            _students.Verify(r => r.AddStudent(It.Is<Student>(s =>
                s.FirstName == dto.FirstName &&
                s.LastName == dto.LastName)), Times.Once);
            _uow.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateStudentAsync_ThrowsKeyNotFoundException_WhenStudentNotFound()
        {
            _students.Setup(r => r.GetStudentByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Student?)null);
            var dto = new StudentEditDto { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", GroupId = Guid.NewGuid() };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateStudentAsync(dto));
        }

        [Fact]
        public async Task UpdateStudentAsync_UpdatesFieldsAndSaves_WhenStudentExists()
        {
            var student = new Student { Id = Guid.NewGuid(), FirstName = "Old", LastName = "Name", GroupId = Guid.NewGuid() };
            _students.Setup(r => r.GetStudentByIdAsync(student.Id)).ReturnsAsync(student);
            var dto = new StudentEditDto { Id = student.Id, FirstName = "New", LastName = "Name", GroupId = student.GroupId };

            await _sut.UpdateStudentAsync(dto);

            Assert.Equal("New", student.FirstName);
            _students.Verify(r => r.UpdateStudent(student), Times.Once);
            _uow.Verify(u => u.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteStudentAsync_ThrowsKeyNotFoundException_WhenStudentNotFound()
        {
            _students.Setup(r => r.GetStudentByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Student?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteStudentAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteStudentAsync_DeletesAndSaves_WhenStudentExists()
        {
            var student = new Student { Id = Guid.NewGuid(), FirstName = "A", LastName = "B", GroupId = Guid.NewGuid() };
            _students.Setup(r => r.GetStudentByIdAsync(student.Id)).ReturnsAsync(student);

            await _sut.DeleteStudentAsync(student.Id);

            _students.Verify(r => r.DeleteStudent(student), Times.Once);
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
}