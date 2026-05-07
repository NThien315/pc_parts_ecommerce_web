using System;
using System.Linq;
using System.Web.Mvc;
using PCPartsWeb.Models;

namespace PCPartsWeb.Controllers
{
    public class ProductsApiController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        // API trả về danh sách sản phẩm dưới dạng JSON
        // Đường dẫn gọi: https://localhost:xxxx/ProductsApi/GetAll
        [HttpGet]
        public ActionResult GetAll()
        {
            // Lưu ý: Phải chọn lọc trường (Select) để tránh lỗi tham chiếu vòng (Circular Reference) của EF
            var products = db.Products.Select(p => new {
                p.ProductID,
                p.ProductName,
                CategoryName = p.Category.CategoryName, // Lấy tên danh mục
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Image = p.Images
            }).ToList();

            // Cho phép các trang web khác gọi API này (CORS - tuỳ chọn)
            return Json(new { success = true, data = products }, JsonRequestBehavior.AllowGet);
        }
    }
}