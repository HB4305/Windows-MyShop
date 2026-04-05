namespace MyShop.Models.ReportModels;

public class ReportOverview
{
  public decimal Revenue { get; set; }

  public decimal PreviousRevenue { get; set; }

  public long QuantitySold { get; set; }

  public long PreviousQuantitySold { get; set; }

  public decimal Profit { get; set; }

  public decimal PreviousProfit { get; set; }

  public int CustomersCount { get; set; }

  public int PreviousCustomersCount { get; set; }

  public decimal RevenueChangePercentage =>
    PreviousRevenue == 0
      ? 0
      : (Revenue - PreviousRevenue) / PreviousRevenue * 100;

  public decimal QuantitySoldChangePercentage =>
    PreviousQuantitySold == 0
      ? 0
      : (decimal)(QuantitySold - PreviousQuantitySold) / PreviousQuantitySold * 100;

  public decimal ProfitChangePercentage =>
    PreviousProfit == 0
      ? 0
      : (Profit - PreviousProfit) / PreviousProfit * 100;

  public decimal CustomersChangePercentage =>
    PreviousCustomersCount == 0
      ? 0
      : (decimal)(CustomersCount - PreviousCustomersCount) / PreviousCustomersCount * 100;
}
