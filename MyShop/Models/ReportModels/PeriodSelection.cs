using MyShop.Utils;

namespace MyShop.Models.ReportModels;

public enum ReportPeriod
{
  Week,
  Month,
  Year
}

public abstract class PeriodSelection
{
  protected DateTime ReferenceDate { get; }

  protected PeriodSelection(DateTime? referenceDate = null)
  {
    ReferenceDate = (referenceDate ?? DateTime.Now).Date;
  }

  public abstract ReportPeriod Period { get; }
  public abstract (DateTime Start, DateTime End) Range { get; }
}

public class WeekPeriodSelection : PeriodSelection
{
  public WeekPeriodSelection(DateTime? referenceDate = null) : base(referenceDate) {}

  public override ReportPeriod Period => ReportPeriod.Week;

  public override (DateTime Start, DateTime End) Range => DateTimeUtils.GetLastWeekRange(ReferenceDate);
}

public class MonthPeriodSelection : PeriodSelection
{
  public MonthPeriodSelection(DateTime? referenceDate = null) : base(referenceDate) {}

  public override ReportPeriod Period => ReportPeriod.Month;

  public override (DateTime Start, DateTime End) Range => DateTimeUtils.GetLastMonthRange(ReferenceDate);
}

public class YearPeriodSelection : PeriodSelection
{
  public YearPeriodSelection(DateTime? referenceDate = null) : base(referenceDate) {}

  public override ReportPeriod Period => ReportPeriod.Year;

  public override (DateTime Start, DateTime End) Range => DateTimeUtils.GetLastYearRange(ReferenceDate);
}
