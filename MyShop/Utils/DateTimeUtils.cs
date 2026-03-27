namespace MyShop.Utils;

public static class DateTimeUtils
{
    public static (DateTime Start, DateTime End) GetDayRange(DateTime referenceTime)
    {
        var start = referenceTime.Date;
        return (start, start.AddDays(1));
    }
}
