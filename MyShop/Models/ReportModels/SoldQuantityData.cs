using System.Text.Json.Serialization;

namespace MyShop.Models.ReportModels;

public class SoldQuantityData
{
  [JsonPropertyName("date")]
  public DateTime Date { get; set; }

  [JsonPropertyName("quantity_sold")]
  public long QuantitySold { get; set; }
}
