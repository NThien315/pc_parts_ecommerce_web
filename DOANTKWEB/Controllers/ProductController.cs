using DOANTKWEB.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DOANTKWEB.Controllers
{
    public class ProductController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        // ========== DANH SÁCH SẢN PHẨM ==========
        public ActionResult Index(string search, int? categoryId, int? brandId)
        {
            // 1. Chặn quyền Admin (Bắt buộc)
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            // 2. Query cơ bản (Dùng Include để lấy luôn tên Danh mục/Thương hiệu)
            var products = db.Products.Include(p => p.Category).Include(p => p.Brand).AsQueryable();

            // 3. Xử lý Lọc & Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId);
            }
            if (brandId.HasValue)
            {
                products = products.Where(p => p.BrandID == brandId);
            }

            // 4. Gửi dữ liệu cho DropdownList
            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "CategoryName", categoryId);
            ViewBag.Brands = new SelectList(db.Brands, "BrandID", "BrandName", brandId);

            // Lưu lại từ khóa tìm kiếm để hiện lại trên ô input
            ViewBag.CurrentSearch = search;

            // 5. Trả về kết quả
            return View(products.OrderByDescending(p => p.ProductID).ToList());
        }

        // ========== CHI TIẾT SẢN PHẨM ==========
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            // 1. Lấy sản phẩm chính
            var product = db.Products.Include("Category").Include("Brand")
                                     .FirstOrDefault(p => p.ProductID == id);

            if (product == null) return HttpNotFound();

            // 2. Lấy sản phẩm liên quan (Cùng Category, khác ID hiện tại, lấy 4 cái)
            ViewBag.RelatedProducts = db.Products
                .Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID)
                .Take(4)
                .ToList();

            return View(product);
        }

        // ========== TẠO SẢN PHẨM ==========
        public ActionResult Create()
        {
            if (Session["Role"] as string != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.BrandID = new SelectList(db.Brands, "BrandID", "BrandName");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Cho phép nhập mô tả có HTML
        public ActionResult Create(Product product, HttpPostedFileBase ImageUpload)
        {
            if (Session["Role"] as string != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {
                // 1. XỬ LÝ ẢNH
                if (ImageUpload != null && ImageUpload.ContentLength > 0)
                {
                    // Đặt tên file
                    string fileName = Path.GetFileNameWithoutExtension(ImageUpload.FileName);
                    string extension = Path.GetExtension(ImageUpload.FileName);
                    fileName = fileName + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + extension;

                    // Lưu vào folder ~/Images/
                    product.Images = fileName;
                    ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Images/"), fileName));
                }
                else
                {
                    product.Images = "default.jpg"; // Ảnh mặc định nếu không up
                }

                // 2. LƯU DATABASE
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Nếu lỗi, load lại dropdown
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.BrandID = new SelectList(db.Brands, "BrandID", "BrandName", product.BrandID);
            return View(product);
        }

        // ========== CHỈNH SỬA ==========
        // Trong Action: Edit (GET)
        public ActionResult Edit(int? id)
        {
            // 1. Kiểm tra ID hợp lệ
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 2. Tìm sản phẩm trong DB
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // 3. ★ QUAN TRỌNG: Chuẩn bị dữ liệu cho DropdownList ★
            // Lỗi xảy ra là do thiếu 2 dòng này
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.BrandID = new SelectList(db.Brands, "BrandID", "BrandName", product.BrandID);

            // 4. Trả về View cùng dữ liệu
            return View(product);
        }

        // Trong Action: Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Product product, HttpPostedFileBase ImageUpload)
        {
            if (Session["Role"] as string != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            if (ModelState.IsValid)
            {

                // 1. KIỂM TRA NẾU CÓ UPLOAD ẢNH MỚI
                if (ImageUpload != null && ImageUpload.ContentLength > 0)
                {
                    // Xóa logic ảnh cũ nếu cần (Tùy chọn)

                    // Lưu ảnh mới
                    string fileName = Path.GetFileNameWithoutExtension(ImageUpload.FileName);
                    string extension = Path.GetExtension(ImageUpload.FileName);
                    fileName = fileName + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + extension;

                    product.Images = fileName; // Cập nhật tên ảnh mới
                    ImageUpload.SaveAs(Path.Combine(Server.MapPath("~/Images/"), fileName));
                }

                // 2. CẬP NHẬT
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.BrandID = new SelectList(db.Brands, "BrandID", "BrandName", product.BrandID);
            return View(product);
        }

        // ========== XÓA SẢN PHẨM ==========
        public ActionResult Delete(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            Product product = db.Products.Find(id);

            // Kiểm tra xem có tìm thấy sản phẩm không trước khi xóa
            if (product != null)
            {
                // Xóa file ảnh cũ nếu có
                if (!string.IsNullOrEmpty(product.Images))
                {
                    string fullPath = Request.MapPath("~/Images/" + product.Images);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                db.Products.Remove(product);
                db.SaveChanges();
                TempData["Success"] = "Đã xóa sản phẩm!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy sản phẩm để xóa!";
            }

            return RedirectToAction("Index");
        }

        public ActionResult GetComponents(string partType, string search, int page = 1)
        {
            int categoryId;
            switch (partType.ToLower())
            {
                case "cpu": categoryId = 5; break;
                case "main": categoryId = 6; break;
                case "ram": categoryId = 7; break;
                case "gpu": categoryId = 8; break;
                case "ssd": categoryId = 9; break;
                case "psu": categoryId = 10; break;
                case "case": categoryId = 11; break;
                default: return Content("Vui lòng chọn loại linh kiện.");
            }

            // Logic Lấy dữ liệu và Phân trang (Giả lập)
            var query = db.Products.Where(p => p.CategoryID == categoryId);

            if (!string.IsNullOrEmpty(search))
            {
                string keyword = search.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(keyword));
            }

            int pageSize = 5;
            var products = query.OrderBy(p => p.ProductID)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            ViewBag.PartType = partType;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = Math.Ceiling((double)query.Count() / pageSize);

            return PartialView("_ComponentList", products);
        }
    }
}