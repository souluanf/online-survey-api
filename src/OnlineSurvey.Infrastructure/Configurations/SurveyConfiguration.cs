using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineSurvey.Domain.Entities;

namespace OnlineSurvey.Infrastructure.Configurations;

public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.ToTable("Surveys");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.Status)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.HasMany(s => s.Questions)
            .WithOne()
            .HasForeignKey(q => q.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Responses)
            .WithOne()
            .HasForeignKey(r => r.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(s => s.AccessMode).IsRequired();
        builder.Property(s => s.CollectedFields).IsRequired();
        builder.Property(s => s.IsPublic).IsRequired();

        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CreatedAt);
    }
}
