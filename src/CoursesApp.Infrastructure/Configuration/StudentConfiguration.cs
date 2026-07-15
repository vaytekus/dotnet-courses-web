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

        builder.Property(s => s.TaxId).HasMaxLength(10).IsFixedLength();
        builder.Property(s => s.Email).HasMaxLength(100);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.City).HasMaxLength(60);
        builder.Property(s => s.Street).HasMaxLength(120);
        builder.Property(s => s.PostalCode).HasMaxLength(10);
        builder.Property(s => s.Gender).HasConversion<string>().HasMaxLength(10);

        builder.HasIndex(s => s.TaxId).IsUnique().HasFilter("[TaxId] IS NOT NULL");
        builder.HasIndex(s => s.Email);
    }
}