using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PCPartsWeb.Models
{
    public class Cart
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Navigation
        public virtual User User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}