using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Food_and_Beverage.Models
{
    internal class Orders
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } // "Cash" or "GCash"
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int RecipeId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string SpecialRequest { get; set; }
    }

}
