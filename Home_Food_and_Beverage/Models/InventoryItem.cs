using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Food_and_Beverage.Models
{
    internal class InventoryItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public int LowStockThreshold { get; set; }
        public DateTime LastUpdated { get; set; }

        public string Category { get; set; }
        public string DietaryStatus { get; set; }
        public string StorageLocation { get; set; }
        public decimal UnitCost { get; set; }

        public bool IsLowStock => Quantity <= LowStockThreshold;
        public decimal StockValue => Quantity * UnitCost;


    }
}
