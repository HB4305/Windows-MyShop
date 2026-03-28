using Newtonsoft.Json;

namespace MyShop.Models.ReportModels;

public abstract class ProductSale
{
  [JsonProperty("quantity_sold")]
  public int QuantitySold { get; set; } = 0;

  [JsonProperty("gross_revenue")]
  public decimal GrossRevenue { get; set; } = 0;

  public string PeriodLabel => GetPeriod();
  public abstract string GetPeriod();
}

public class ProductSaleByDay : ProductSale
{
  [JsonProperty("day")]
  public DateTime Day { get; set; }

  public override string GetPeriod() => Day.ToString("yyyy-MM-dd");
}

public class ProductSaleByWeek : ProductSale
{
  [JsonProperty("start_date")]
  public DateTime StartDate { get; set; }

  [JsonProperty("end_date")]
  public DateTime EndDate { get; set; }

  public override string GetPeriod() => $"{StartDate:yyyy-MM-dd}\nto\n{EndDate:yyyy-MM-dd}";
}

public class ProductSaleByMonth : ProductSale
{
  [JsonProperty("year")]
  public int Year { get; set; }

  [JsonProperty("month")]
  public int Month { get; set; }

  public override string GetPeriod() => $"{Year}-{Month:00}";
}

public class ProductSaleByYear : ProductSale
{
  [JsonProperty("year")]
  public int Year { get; set; }

  public override string GetPeriod() => Year.ToString();
}
