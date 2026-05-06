using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOANTKWEB.Models
{
    public class Review
    {
        public int ReviewID { get; set; }

        public int ProductID { get; set; }
        public int UserID { get; set; }

        public int Rating { get; set; } // 1–5
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; }

        // Navigation
        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
    }
}