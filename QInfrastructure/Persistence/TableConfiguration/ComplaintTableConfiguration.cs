using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QDomain.Models;

namespace QInfrastructure.Persistence.TableConfiguration;

public class ComplaintTableConfiguration: IEntityTypeConfiguration<ComplaintEntity>
{
    public void Configure(EntityTypeBuilder<ComplaintEntity> builder)
    {
        builder.ToTable("Complaints");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.CustomerId);
        

        builder.HasOne(s => s.Queue)
            .WithMany()
            .HasForeignKey(s => s.QueueId);
    }
}