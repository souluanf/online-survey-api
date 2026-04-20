using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineSurvey.Domain.Entities;

namespace OnlineSurvey.Infrastructure.Configurations;

public class SurveyAccessCodeConfiguration : IEntityTypeConfiguration<SurveyAccessCode>
{
    public void Configure(EntityTypeBuilder<SurveyAccessCode> builder)
    {
        builder.ToTable("SurveyAccessCodes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
        builder.Property(c => c.CodeHash).IsRequired().HasMaxLength(64);
        builder.HasIndex(c => new { c.SurveyId, c.Email });
    }
}
