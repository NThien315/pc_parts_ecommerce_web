using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PCPartsWeb.Models
{
    public class CategoryStatus
    {
        public Category Category { get; set; }

        // Cho biết danh mục này hiện tại có sản phẩm hay không
        public bool HasProducts { get; set; }
    }
}