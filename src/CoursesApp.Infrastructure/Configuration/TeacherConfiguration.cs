using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursesApp.Infrastructure.Configuration;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder
            .Property(t => t.FirstName)
            .IsRequired().HasMaxLength(50);
            
        builder
            .Property(t => t.LastName)
            .IsRequired().HasMaxLength(50);

        builder.Property(t => t.Email).HasMaxLength(100);
        builder.Property(t => t.Phone).HasMaxLength(30);
        builder.Property(t => t.Degree).HasMaxLength(100);

        builder.HasIndex(t => t.Email);
    }
}
