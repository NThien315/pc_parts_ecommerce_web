using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DOANTKWEB.Models
{
    public class HomeViewModel
    {
        // ====== CÁC PHẦN CŨ Ở INDEX ======
        public List<Product> DiscountProducts { get; set; }   // Sản phẩm giảm giá
        public List<Product> GamingPCs { get; set; }          // PC Gaming
        public List<Product> OfficePCs { get; set; }          // PC Văn phòng
        public List<Product> DesignPCs { get; set; }          // PC Đồ họa / Design
        public List<Product> LaptopProducts { get; set; }     // Laptop
        public List<Product> NewProducts { get; set; }        // Hàng mới
        public List<Brand> FeaturedBrands { get; set; }       // Thương hiệu

        // ====== PHẦN DANH MỤC + LỌC SẢN PHẨM ======
        // Danh sách danh mục + trạng thái có sản phẩm hay không
        public List<CategoryStatus> CategoryList { get; set; }

        // Sản phẩm được lọc theo danh mục (hiển thị ở khu vực "Danh mục sản phẩm")
        public List<Product> FilteredProducts { get; set; }

        // Để biết đang chọn danh mục nào (dùng CSS active)
        public int? SelectedCategoryId { get; set; }
    }
}