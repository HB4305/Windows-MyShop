namespace MyShop.Utils;

public static class DateTimeUtils
{
    public static (DateTime Start, DateTime End) GetDayRange(DateTime referenceTime)
    {
        // Convert local date boundaries to UTC for correct DB comparison
        // DB stores created_at as UTC (DateTimeOffset.UtcNow)
        var localStart = referenceTime.Date;
        var localEnd = localStart.AddDays(1);
        var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart);
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd);
        return (utcStart, utcEnd);
    }

    public static (DateTime Start, DateTime End) GetLastWeekRange(DateTime? referenceTime = null)
    {
        var today = (referenceTime ?? DateTime.Today).Date;
        int daysSinceMonday = (int)today.DayOfWeek - (int)DayOfWeek.Monday;

        if (daysSinceMonday < 0)
        {
            daysSinceMonday += 7;
        }

        var thisMonday = today.AddDays(-daysSinceMonday);
        var start = thisMonday.AddDays(-7);
        var end = thisMonday;

        return (start, end);
    }

    public static (DateTime Start, DateTime End) GetLastMonthRange(DateTime? referenceTime = null)
    {
        var today = (referenceTime ?? DateTime.Today).Date;
        var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
        var start = firstDayThisMonth.AddMonths(-1);
        var end = firstDayThisMonth;

        return (start, end);
    }

    public static (DateTime Start, DateTime End) GetLastYearRange(DateTime? referenceTime = null)
    {
        var today = (referenceTime ?? DateTime.Today).Date;
        var start = new DateTime(today.Year - 1, 1, 1);
        var end = new DateTime(today.Year, 1, 1);

        return (start, end);
    }
}
