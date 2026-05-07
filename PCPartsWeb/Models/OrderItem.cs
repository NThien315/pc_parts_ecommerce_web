using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PCPartsWeb.Models
{
    public class OrderItem
    {
        public int OrderItemID { get; set; }

        public int OrderID { get; set; }
        public int ProductID { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Navigation
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}