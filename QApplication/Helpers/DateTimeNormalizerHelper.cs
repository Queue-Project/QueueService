namespace QApplication.Helpers;

public static class DateTimeNormalizeHelper
{
    public static DateTimeOffset Normalize(DateTimeOffset value)
    {
        return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Offset);
    }
}