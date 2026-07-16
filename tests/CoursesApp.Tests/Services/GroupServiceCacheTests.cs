using Microsoft.Extensions.Caching.Memory;

namespace CoursesApp.Tests.Services;

public class GroupServiceCacheTests
{
    private const string _teachersCacheKey = "teachers_all";
    private const string _coursesCacheKey = "courses_all";

    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IGroupRepository> _groups;
    private readonly IMemoryCache _cache;
    private readonly GroupService _sut;

    public GroupServiceCacheTests()
    {
        _uow = new Mock<IUnitOfWork>();
        _groups = new Mock<IGroupRepository>();
        var logger = new Mock<ILogger<GroupService>>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _uow.Setup(u => u.Groups).Returns(_groups.Object);
        _sut = new GroupService(_uow.Object, _cache, logger.Object);
    }

    [Fact]
    public void InvalidateTeachersCache_RemovesTeachersCacheKey()
    {
        _cache.Set(_teachersCacheKey, new List<Teacher>
        {
            new() { Id = Guid.NewGuid(), FirstName = "A", LastName = "B" }
        });

        _sut.InvalidateTeachersCache();

        Assert.False(_cache.TryGetValue(_teachersCacheKey, out _));
    }

    [Fact]
    public void InvalidateTeachersCache_DoesNotRemoveCoursesCacheKey()
    {
        var courses = new List<Course> { new() { Id = Guid.NewGuid(), Name = "Math", Description = "desc" } };
        _cache.Set(_coursesCacheKey, courses);

        _sut.InvalidateTeachersCache();

        Assert.True(_cache.TryGetValue(_coursesCacheKey, out List<Course>? cached));
        Assert.Same(courses, cached);
    }

    [Fact]
    public async Task UnassignTeacherAsync_InvalidatesTeachersCache()
    {
        var teacherId = Guid.NewGuid();
        _cache.Set(_teachersCacheKey, new List<Teacher>
        {
            new() { Id = teacherId, FirstName = "A", LastName = "B" }
        });
        _groups.Setup(r => r.UnassignTeacherAsync(teacherId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.UnassignTeacherAsync(teacherId);

        Assert.False(_cache.TryGetValue(_teachersCacheKey, out _));
        _groups.Verify(r => r.UnassignTeacherAsync(teacherId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
