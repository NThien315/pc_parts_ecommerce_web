using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PCPartsWeb.Models;

namespace PCPartsWeb.Controllers
{
    public class BrandController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        // GET: Brand (Hiển thị danh sách)
        public ActionResult Index()
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");
            return View(db.Brands.ToList());
        }

        // POST: Brand/Create (Xử lý Form từ Modal Thêm)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Brand brand)
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                db.Brands.Add(brand);
                db.SaveChanges();
                TempData["Success"] = "Thêm thương hiệu thành công!";
            }
            else
            {
                TempData["Error"] = "Vui lòng kiểm tra lại thông tin nhập!";
            }
            return RedirectToAction("Index");
        }

        // POST: Brand/Edit (Xử lý Form từ Modal Sửa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Brand brand)
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                db.Entry(brand).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thành công!";
            }
            return RedirectToAction("Index");
        }

        // POST: Brand/Delete (Xử lý Xóa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            var brand = db.Brands.Find(id);
            if (brand != null)
            {
                // Kiểm tra ràng buộc: Nếu Brand đang có sản phẩm thì không cho xóa
                if (db.Products.Any(p => p.BrandID == id))
                {
                    TempData["Error"] = "Không thể xóa! Thương hiệu này đang có sản phẩm.";
                }
                else
                {
                    db.Brands.Remove(brand);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa thương hiệu!";
                }
            }
            return RedirectToAction("Index"); // Quay về trang Index
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}