using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DOANTKWEB.Models;
using PagedList;

namespace DOANTKWEB.Controllers
{
    public class HomeController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        public ActionResult Index(string search, int? categoryId)
        {
            // 1. QUERY CHÍNH (Dùng cho tìm kiếm & lọc)
            var products = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search) || p.Brand.BrandName.Contains(search));
                ViewBag.CurrentSearch = search;
            }
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId);
            }

            // 2. NHÉT DỮ LIỆU CÁC MỤC VÀO VIEWBAG

            ViewBag.DiscountProducts = db.Products.Where(p => p.DiscountPrice.HasValue && p.DiscountPrice < p.Price).Take(8).ToList();
            ViewBag.GamingPCs = db.Products.Where(p => p.CategoryID == 1).Take(8).ToList();
            ViewBag.OfficePCs = db.Products.Where(p => p.CategoryID == 2).Take(8).ToList();
            ViewBag.DesignPCs = db.Products.Where(p => p.CategoryID == 3).Take(8).ToList();
            ViewBag.LaptopProducts = db.Products.Where(p => p.CategoryID == 4).Take(8).ToList();

            // Trả về danh sách kết quả tìm kiếm (nếu có)
            return View(products.ToList());
        }

        // Action tìm kiếm riêng 
        public ActionResult Search(string keyword, decimal? minPrice, decimal? maxPrice)
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Support()
        {
            return View();
        }
        public ActionResult Installment()
        {
            return View();
        }
        public ActionResult BuildPCView()
        {
            return View();
        }

        // Liên hệ - Xử lý gửi form
        [HttpPost]
        public JsonResult Send(string name, string email, string phone, string message)
        {
            // Giả lập thành công để Frontend báo "OK"

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Thông tin không hợp lệ!" });
            }

            // Giả sử xử lý thành công
            return Json(new { success = true, message = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm!" });
        }

        public ActionResult Shop(string search, string sortOrder, int? page)
        {
            // 1. Lưu từ khóa tìm kiếm để hiện lại trên View
            ViewBag.CurrentSearch = search;

            // 2. Thiết lập sắp xếp (Sử dụng tham số từ URL)
            ViewBag.CurrentSort = sortOrder;

            // Khởi tạo truy vấn và Include Category để sắp xếp/tìm kiếm theo CategoryName
            var products = db.Products.Include(p => p.Category).AsQueryable();

            // 3. Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search)
                                             || p.Category.CategoryName.Contains(search));
            }

            // 4. Sắp xếp (Áp dụng logic mới)
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;

                case "Price": // Giá: Tăng dần
                              // Sắp xếp theo giá khuyến mãi (DiscountPrice) nếu có, ngược lại dùng Price
                    products = products.OrderBy(p => p.DiscountPrice.HasValue ? p.DiscountPrice.Value : p.Price);
                    break;

                case "price_desc": // Giá: Giảm dần
                    products = products.OrderByDescending(p => p.DiscountPrice.HasValue ? p.DiscountPrice.Value : p.Price);
                    break;

                case "cate": // Sắp xếp theo Danh mục (Đã thêm ở bước trước)
                    products = products.OrderBy(p => p.Category.CategoryName).ThenBy(p => p.ProductName);
                    break;

                default: // Mặc định: Tên A-Z
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }

            // 5. Phân trang
            int pageSize = 12;
            int pageNumber = (page ?? 1);

            return View(products.ToPagedList(pageNumber, pageSize));
        }
    }
}
