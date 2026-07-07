using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursesApp.Infrastructure.Configuration;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder
            .Property(s => s.FirstName)
            .IsRequired().HasMaxLength(50);
        
        builder
            .Property(s => s.LastName)
            .IsRequired().HasMaxLength(50);
        
        builder
            .HasOne(s => s.Group)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
