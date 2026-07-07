using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursesApp.Infrastructure.Configuration;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder
            .Property(c => c.Name)
            .IsRequired().HasMaxLength(100);
        
        builder
            .HasOne(g => g.Course)
            .WithMany(c => c.Groups)
            .HasForeignKey(g => g.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasOne(g => g.Teacher)
            .WithMany(t => t.Groups)
            .HasForeignKey(g => g.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
