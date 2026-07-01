using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
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
            _students.Verify(r => r.AddStudent(It.IsAny<Student>()), Times.Exactly(2));
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
            _students.Verify(r => r.AddStudent(It.Is<Student>(s => s.FirstName == "Alice")), Times.Once);
            _students.Verify(r => r.AddStudent(It.Is<Student>(s => s.FirstName == "Bob")), Times.Once);
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
}