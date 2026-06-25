using CoursesApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoursesApp.Infrastructure.Configuration
{
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
        }
    }
}