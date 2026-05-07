using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PCPartsWeb.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ProductName { get; set; }

        public int CategoryID { get; set; }
        public int BrandID { get; set; }

        public string Description { get; set; }
        public string CPU { get; set; }
        public string GPU { get; set; }
        public string RAM { get; set; }
        public string Storage { get; set; }
        public string PSU { get; set; }
        public string Mainboard { get; set; }
        public string Case { get; set; }

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public int StockQuantity { get; set; }
        public int WarrantyMonths { get; set; }

        public string Images { get; set; } // JSON hoặc chuỗi tách bằng dấu |

        // Navigation
        public virtual Category Category { get; set; }
        public virtual Brand Brand { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}