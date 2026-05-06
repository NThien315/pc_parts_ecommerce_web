using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOANTKWEB.Models
{
    public class CartItem
    {
        public int CartItemID { get; set; }

        public int CartID { get; set; }
        public int ProductID { get; set; }

        public int Quantity { get; set; }

        // Navigation
        public virtual Cart Cart { get; set; }
        public virtual Product Product { get; set; }
    }
}