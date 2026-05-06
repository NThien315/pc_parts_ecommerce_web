using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOANTKWEB.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public string PasswordHash { get; set; }
        public string Role { get; set; } // Admin / User

        // Navigation
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}