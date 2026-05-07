using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PCPartsWeb.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public int? ParentID { get; set; }

        public virtual ICollection<Product> Products { get; set; }
        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; }
    }
}