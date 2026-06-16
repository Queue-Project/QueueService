using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QDomain.Models;

namespace QInfrastructure.Persistence.TableConfiguration;

public class QueueTableConfiguration: IEntityTypeConfiguration<QueueEntity>
{
    public void Configure(EntityTypeBuilder<QueueEntity> builder)
    {
        builder.ToTable("Queues");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.CompanyId);
        builder.HasIndex(s => s.BranchId);
        builder.HasIndex(s => s.ServiceId);
        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.EmployeeId);

        
    }
}