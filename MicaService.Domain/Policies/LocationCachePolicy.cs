namespace MicaService.Domain.Policies;

public static class LocationCachePolicy
{
    private static readonly int[] ScheduleHours = [6, 12, 18];

    public static DateTimeOffset GetNextSchedule(DateTimeOffset now)
    {
        var date = DateOnly.FromDateTime(now.DateTime);
        if (IsWeekend(date))
        {
            date = NextWeekday(date);
            return new DateTimeOffset(
                date.Year, date.Month, date.Day, ScheduleHours[0], 0, 0, now.Offset);
        }

        foreach (var hour in ScheduleHours)
        {
            var candidate = new DateTimeOffset(
                date.Year, date.Month, date.Day, hour, 0, 0, now.Offset);
            if (candidate > now)
            {
                return candidate;
            }
        }

        date = NextWeekday(date.AddDays(1));
        return new DateTimeOffset(
            date.Year, date.Month, date.Day, ScheduleHours[0], 0, 0, now.Offset);
    }

    private static DateOnly NextWeekday(DateOnly date)
    {
        var cursor = date;
        while (IsWeekend(cursor))
        {
            cursor = cursor.AddDays(1);
        }

        return cursor;
    }

    private static bool IsWeekend(DateOnly date)
    {
        var day = date.DayOfWeek;
        return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
    }
}
