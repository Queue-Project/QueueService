using Microsoft.EntityFrameworkCore.ChangeTracking;
using QAuditLogService.Contracts;

namespace QApplication.Helpers;

public static class AuditHelper
{
    public static List<AuditEventLogDetails> GetChanges(EntityEntry entry)
    {
        var changes = new List<AuditEventLogDetails>();

        foreach (var property in entry.Properties)
        {
            if (!property.IsModified)
                continue;

            changes.Add(new AuditEventLogDetails
            {
                PropertyName = property.Metadata.Name,
                OldValue = property.OriginalValue?.ToString(),
                NewValue = property.CurrentValue?.ToString()
            });
        }

        return changes;
    }
}