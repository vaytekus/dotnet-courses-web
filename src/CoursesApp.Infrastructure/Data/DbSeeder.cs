using Bogus;

namespace CoursesApp.Infrastructure.Data;

public static class DbSeeder
{
    private const int _teachersCount = 5;
    private const int _studentsPerGroup = 20;
    private const string _locale = "uk";
    private const int _randomSeed = 42;

    private static readonly string[] _degrees =
    [
        "BSc Computer Science",
        "MSc Computer Science",
        "PhD in Software Engineering",
        "MSc Mathematics",
        "BSc Information Technology"
    ];

    public static async Task SeedAsync(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        await db.Database.MigrateAsync();

        Randomizer.Seed = new Random(_randomSeed);

        await SeedTeachersAsync(db);
        await SeedCoursesAsync(db);
    }

    private static async Task SeedTeachersAsync(AppDbContext db)
    {
        if (await db.Teachers.AnyAsync())
        {
            return;
        }

        var faker = new Faker<Teacher>(_locale)
            .RuleFor(t => t.Id, _ => Guid.NewGuid())
            .RuleFor(t => t.FirstName, f => f.Name.FirstName())
            .RuleFor(t => t.LastName, f => f.Name.LastName())
            .RuleFor(t => t.DateOfBirth, f => DateOnly.FromDateTime(
                f.Date.Between(DateTime.Today.AddYears(-60), DateTime.Today.AddYears(-28))))
            .RuleFor(t => t.Email, (f, t) => f.Internet.Email(t.FirstName, t.LastName).ToLowerInvariant())
            .RuleFor(t => t.Phone, f => f.Phone.PhoneNumber("+380#########"))
            .RuleFor(t => t.HireDate, f => DateOnly.FromDateTime(
                f.Date.Between(DateTime.Today.AddYears(-10), DateTime.Today.AddYears(-1))))
            .RuleFor(t => t.Degree, f => f.PickRandom(_degrees));

        db.AddRange(faker.Generate(_teachersCount));
        await db.SaveChangesAsync();
    }

    private static async Task SeedCoursesAsync(AppDbContext db)
    {
        if (await db.Courses.AnyAsync())
        {
            return;
        }

        var teachers = await db.Teachers.ToListAsync();
        var picker = new Faker();

        var csharp = new Course
        {
            Id = Guid.NewGuid(), Name = "C# Basics", Description = "Intro to C#",
            Level = CourseLevel.Beginner, DurationWeeks = 12, Category = "Programming"
        };
        var sql = new Course
        {
            Id = Guid.NewGuid(), Name = "SQL", Description = "Relational databases",
            Level = CourseLevel.Intermediate, DurationWeeks = 8, Category = "Databases"
        };
        var js = new Course
        {
            Id = Guid.NewGuid(), Name = "JavaScript", Description = "Web programming",
            Level = CourseLevel.Beginner, DurationWeeks = 10, Category = "Web"
        };

        var groupA = new Group
        {
            Id = Guid.NewGuid(), Name = "C#-Spring-2026", Course = csharp,
            Teacher = picker.PickRandom(teachers),
            StartDate = new DateOnly(2026, 3, 1), EndDate = new DateOnly(2026, 5, 31),
            MaxCapacity = picker.Random.Int(20, 30)
        };
        var groupB = new Group
        {
            Id = Guid.NewGuid(), Name = "SQL-Summer-2026", Course = sql,
            Teacher = picker.PickRandom(teachers),
            StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 8, 31),
            MaxCapacity = picker.Random.Int(20, 30)
        };
        var groupC = new Group
        {
            Id = Guid.NewGuid(), Name = "JS-Autumn-2026", Course = js,
            Teacher = picker.PickRandom(teachers),
            StartDate = new DateOnly(2026, 9, 1), EndDate = new DateOnly(2026, 11, 30),
            MaxCapacity = picker.Random.Int(20, 30)
        };

        var usedTaxIds = new HashSet<string>();
        string uniqueTaxId(Faker f)
        {
            string id;
            do { id = f.Random.ReplaceNumbers("##########"); } while (!usedTaxIds.Add(id));
            return id;
        }

        var studentFaker = new Faker<Student>(_locale)
            .RuleFor(s => s.Id, _ => Guid.NewGuid())
            .RuleFor(s => s.FirstName, f => f.Name.FirstName())
            .RuleFor(s => s.LastName, f => f.Name.LastName())
            .RuleFor(s => s.DateOfBirth, f => DateOnly.FromDateTime(
                f.Date.Between(DateTime.Today.AddYears(-30), DateTime.Today.AddYears(-18))))
            .RuleFor(s => s.Gender, f => f.PickRandom<Gender>())
            .RuleFor(s => s.TaxId, uniqueTaxId)
            .RuleFor(s => s.Email, (f, s) => f.Internet.Email(s.FirstName, s.LastName).ToLowerInvariant())
            .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber("+380#########"))
            .RuleFor(s => s.City, f => f.Address.City())
            .RuleFor(s => s.Street, f => f.Address.StreetAddress())
            .RuleFor(s => s.PostalCode, f => f.Address.ZipCode("#####"));

        foreach (var group in new[] { groupA, groupB, groupC })
        {
            foreach (var s in studentFaker.Generate(_studentsPerGroup))
            {
                group.Students.Add(s);
            }
        }

        db.AddRange(csharp, sql, js, groupA, groupB, groupC);
        await db.SaveChangesAsync();
    }
}