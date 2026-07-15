using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursesApp.Infrastructure.Configuration;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder
            .Property(c => c.Name)
            .IsRequired().HasMaxLength(100);
        
        builder
            .Property(c => c.Description)
            .IsRequired().HasMaxLength(500);
        
        builder.Property(c => c.Level).HasConversion<string>().HasMaxLength(15);
        builder.Property(c => c.Category).HasMaxLength(60);
    }
}
