using Newtonsoft.Json;

namespace MyShop.Models.ReportModels;

public class SoldQuantityData
{
  [JsonProperty("date")]
  public DateTime Date { get; set; }

  [JsonProperty("quantity_sold")]
  public long QuantitySold { get; set; }
}
