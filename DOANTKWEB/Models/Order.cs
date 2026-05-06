using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOANTKWEB.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }

        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public string PaymentMethod { get; set; }
        public string Status { get; set; } // Pending / Paid / Shipping / Completed / Cancelled

        // Navigation
        public virtual User User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}