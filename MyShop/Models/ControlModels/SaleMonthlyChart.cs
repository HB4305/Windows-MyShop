using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace MyShop.Models.ControlModels
{
    [Table("SaleMonthlyChart")]
    public class SaleMonthlyChart : BaseModel
    {
        [Column("date")]
        public DateTime Date { get; set; }
        [Column("revenue")]
        public decimal Revenue { get; set; }

    }
}