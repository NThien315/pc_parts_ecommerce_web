using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PCPartsWeb.Models;

namespace PCPartsWeb.Controllers
{
    public class CategoryController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        // GET: Category
        public ActionResult Index()
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");
            return View(db.Categories.ToList());
        }

        // POST: Category/Create (Xử lý Form từ Modal Thêm)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Thêm danh mục thành công!";
            }
            else
            {
                TempData["Error"] = "Tên danh mục không hợp lệ!";
            }
            return RedirectToAction("Index");
        }

        // POST: Category/Edit (Xử lý Form từ Modal Sửa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thành công!";
            }
            return RedirectToAction("Index");
        }

        // POST: Category/Delete (Xử lý Xóa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            var category = db.Categories.Find(id);
            if (category != null)
            {
                // Kiểm tra ràng buộc: Nếu Danh mục đang có sản phẩm thì không cho xóa
                if (db.Products.Any(p => p.CategoryID == id))
                {
                    TempData["Error"] = "Không thể xóa! Danh mục này đang chứa sản phẩm.";
                }
                else
                {
                    db.Categories.Remove(category);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa danh mục!";
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