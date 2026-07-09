using CoursesApp.Web.Controllers;
using CoursesApp.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace CoursesApp.Tests.Controllers;

public class StudentsControllerTests
{
    private readonly Mock<IStudentService> _students;
    private readonly Mock<IClientProxy> _clientProxy;
    private readonly StudentsController _sut;

    public StudentsControllerTests()
    {
        _students = new Mock<IStudentService>();
        _clientProxy = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubClients>();
        var hub = new Mock<IHubContext<AppHub>>();
        mockClients.Setup(c => c.All).Returns(_clientProxy.Object);
        hub.Setup(h => h.Clients).Returns(mockClients.Object);

        var config = new Mock<IConfiguration>();
        var section = new Mock<IConfigurationSection>();
        section.Setup(s => s.Value).Returns((string?)null);
        config.Setup(c => c.GetSection(It.IsAny<string>())).Returns(section.Object);

        _sut = new StudentsController(
            _students.Object,
            new Mock<IGroupService>().Object,
            new Mock<ICsvService>().Object,
            config.Object,
            hub.Object,
            new Mock<ILogger<StudentsController>>().Object);
    }

    [Fact]
    public async Task Add_BroadcastsStudentAdded_WhenDtoIsValid()
    {
        var dto = new StudentDto(null, "John", "Doe", Guid.NewGuid());

        await _sut.Add(dto, CancellationToken.None);

        _clientProxy.Verify(c => c.SendCoreAsync(
            "StudentAdded",
            It.Is<object[]>(args => args[0].Equals(dto.GroupId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Add_DoesNotBroadcast_WhenDtoIsNull()
    {
        await _sut.Add(null!, CancellationToken.None);

        _clientProxy.Verify(c => c.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Edit_BroadcastsStudentUpdated_WhenStudentExists()
    {
        var dto = new StudentEditDto(Guid.NewGuid(), "Jane", "Smith", Guid.NewGuid());

        await _sut.Edit(dto, CancellationToken.None);

        _clientProxy.Verify(c => c.SendCoreAsync(
            "StudentUpdated",
            It.Is<object[]>(args =>
                args[0].Equals(dto.Id) &&
                args[1].Equals(dto.FirstName) &&
                args[2].Equals(dto.LastName) &&
                args[3].Equals(dto.GroupId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_BroadcastsStudentDeleted_WhenStudentExists()
    {
        var id = Guid.NewGuid();
        var groupId = Guid.NewGuid();

        await _sut.Delete(id, groupId, CancellationToken.None);

        _clientProxy.Verify(c => c.SendCoreAsync(
            "StudentDeleted",
            It.Is<object[]>(args =>
                args[0].Equals(id) &&
                args[1].Equals(groupId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
