using DOANTKWEB.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DOANTKWEB.Controllers
{
    public class AdminController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        // ========= KIỂM TRA ADMIN =========
        private bool IsAdmin()
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Admin";
        }

        // ========= Dashboard Admin =========
        public ActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewBag.TotalUsers = db.Users.Count();
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalRevenue =
            db.OrderItems
              .Where(i => i.Order.Status == "Completed") 
              .Sum(i => (decimal?)i.Price * i.Quantity) ?? 0;

            return View();
        }

        // ========= Quản lý User =========
        public ActionResult Users(string search, string role)
        {
            // 1. Kiểm tra quyền Admin
            if (Session["Role"] as string != "Admin")
                return RedirectToAction("Login", "Account");

            // 2. Khởi tạo truy vấn (chưa chạy ngay)
            var query = db.Users.AsQueryable();

            // 3. Xử lý tìm kiếm (Theo Tên hoặc Email)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            }

            // 4. Xử lý lọc theo Quyền (Admin/User)
            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role == role);
            }

            // 5. Lưu lại giá trị để hiển thị lại trên Form (UX)
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentRole = role;

            // 6. Trả về danh sách kết quả (Sắp xếp mới nhất lên đầu)
            return View(query.OrderByDescending(u => u.UserID).ToList());
        }

        // POST: Admin/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id)
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            var user = db.Users.Find(id);
            if (user != null)
            {
                // Đặt mật khẩu mặc định là 123456
                string defaultPass = "123456";
                user.PasswordHash = DOANTKWEB.Helpers.SecurityHelper.HashPassword(defaultPass);

                db.SaveChanges();
                TempData["Success"] = $"Đã reset mật khẩu của {user.FullName} thành: {defaultPass}";
            }
            return RedirectToAction("Users");
        }

        // --- 1. THÊM NGƯỜI DÙNG (CREATE) ---
        public ActionResult UserCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserCreate(DOANTKWEB.Models.User user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra email trùng
                if (db.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã tồn tại!");
                    return View(user);
                }

                // Mặc định mật khẩu là 123456 nếu không nhập 
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    user.PasswordHash = "123456"; // Mặc định
                }

                user.PasswordHash = DOANTKWEB.Helpers.SecurityHelper.HashPassword(user.PasswordHash);

                db.Users.Add(user);
                db.SaveChanges();
                TempData["Success"] = "Đã thêm người dùng mới!";
                return RedirectToAction("Users");
            }
            return View(user);
        }

        // --- 2. SỬA NGƯỜI DÙNG (EDIT) ---
        public ActionResult UserEdit(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserEdit(DOANTKWEB.Models.User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.Find(user.UserID);
                if (existingUser != null)
                {
                    // Cập nhật thông tin (KHÔNG cập nhật mật khẩu ở đây)
                    existingUser.FullName = user.FullName;
                    existingUser.Phone = user.Phone;
                    existingUser.Address = user.Address;
                    existingUser.Role = user.Role; // Cho phép đổi quyền Admin/User

                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Users");
                }
            }
            return View(user);
        }

        // --- 3. XÓA NGƯỜI DÙNG (DELETE) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserDelete(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                // Chặn xóa chính mình (Admin đang đăng nhập)
                var currentUser = Session["User"] as DOANTKWEB.Models.User;
                if (currentUser != null && currentUser.UserID == id)
                {
                    TempData["Error"] = "Không thể tự xóa tài khoản của chính mình!";
                    return RedirectToAction("Users");
                }

                try
                {
                    db.Users.Remove(user);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa người dùng!";
                }
                catch (Exception)
                {
                    // Lỗi này thường do User đã có Đơn hàng -> dính khóa ngoại (Foreign Key)
                    TempData["Error"] = "Không thể xóa user này vì họ đã có dữ liệu mua hàng! (Hãy khóa tài khoản thay vì xóa)";
                }
            }
            return RedirectToAction("Users");
        }
    }
}
