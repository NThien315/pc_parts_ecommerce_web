using DOANTKWEB.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web.Mvc;

namespace DOANTKWEB.Controllers
{
    public class AccountController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        // ========== LOGIN ==========

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UserID"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Email, string Password, bool? RememberMe)
        {
            // 1. Kiểm tra nhập liệu
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["LoginError"] = "Vui lòng nhập Email và Mật khẩu.";
                return RedirectToAction("Login");
            }

            // 2. Mã hóa mật khẩu nhập vào để so sánh với DB
            string hash = DOANTKWEB.Helpers.SecurityHelper.HashPassword(Password);

            // Tìm user có Email trùng VÀ Mật khẩu mã hóa trùng
            var user = db.Users.SingleOrDefault(u => u.Email == Email && u.PasswordHash == hash);

            if (user == null)
            {
                TempData["LoginError"] = "Email hoặc mật khẩu không đúng.";
                return RedirectToAction("Login");
            }

            // 3. LƯU SESSION
            Session["User"] = user;
            Session["UserID"] = user.UserID;
            Session["Role"] = user.Role;

            // 4. Điều hướng
            if (!string.IsNullOrEmpty(user.Role) && user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction("Index", "Home");
        }

        // ========== REGISTER ==========

        [HttpGet]
        public ActionResult Register()
        {
            if (Session["User"] != null) return RedirectToAction("Index", "Home");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(DOANTKWEB.Models.User user, string Password, string ConfirmPassword)
        {
            // Kiểm tra dữ liệu rỗng (Nếu bên Model chưa có [Required])
            if (string.IsNullOrWhiteSpace(user.FullName) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["Error"] = "Vui lòng nhập đủ thông tin!";
                return RedirectToAction("Login", "Account");
            }

            if (Password != ConfirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                return RedirectToAction("Login", "Account");
            }

            if (db.Users.Any(u => u.Email == user.Email))
            {
                TempData["Error"] = "Email đã tồn tại!";
                return RedirectToAction("Login", "Account");
            }

            user.PasswordHash = DOANTKWEB.Helpers.SecurityHelper.HashPassword(Password);
            user.Role = "User";

            db.Users.Add(user);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login", "Account");
        }

        // POST: Account/ChangePassword (Đổi mật khẩu cho người đang đăng nhập)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            // 1. Kiểm tra session
            if (Session["User"] == null)
            {
                return RedirectToAction("Login");
            }

            // 2. Validate dữ liệu
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin!";
                return Redirect(Request.UrlReferrer.ToString());
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                return Redirect(Request.UrlReferrer.ToString());
            }

            // 3. Kiểm tra mật khẩu cũ
            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);
            string currentHash = DOANTKWEB.Helpers.SecurityHelper.HashPassword(CurrentPassword);

            if (user.PasswordHash != currentHash)
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng!";
                return Redirect(Request.UrlReferrer.ToString());
            }

            // 4. Cập nhật mật khẩu mới
            user.PasswordHash = DOANTKWEB.Helpers.SecurityHelper.HashPassword(NewPassword);
            db.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return Redirect(Request.UrlReferrer.ToString());
        }

        // ========== LOGOUT ==========

        public ActionResult Logout()
        {
            Session["User"] = null;
            Session["UserID"] = null;
            Session["Role"] = null;

            return RedirectToAction("Index", "Home");
        }

        // ========== PROFILE ==========

        public ActionResult UserProfile()
        {
            // Kiểm tra đăng nhập
            var user = Session["User"] as DOANTKWEB.Models.User;
            if (user == null) return RedirectToAction("Login", "Account");

            // Lấy UserID một cách an toàn
            int userId = user.UserID;

            // Lấy Lịch sử Đặt hàng
            ViewBag.OrderHistory = db.Orders
                                       .Include("OrderItems.Product")
                                       .Where(o => o.UserID == userId) 
                                       .OrderByDescending(o => o.OrderDate)
                                       .ToList();

            // Trả về model User để hiển thị thông tin
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserProfile(DOANTKWEB.Models.User updatedUser)
        {
            // Kiểm tra đăng nhập
            var currentUser = Session["User"] as DOANTKWEB.Models.User;
            if (currentUser == null) return RedirectToAction("Login", "Account");

            // Đảm bảo dữ liệu form hợp lệ
            if (ModelState.IsValid)
            {
                var userInDb = db.Users.Find(currentUser.UserID);
                if (userInDb != null)
                {
                    // Cập nhật các trường cho phép: Tên, SĐT, Địa chỉ
                    userInDb.FullName = updatedUser.FullName;
                    userInDb.Phone = updatedUser.Phone;
                    userInDb.Address = updatedUser.Address;
                    // Không cập nhật Email và Role

                    db.Entry(userInDb).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    Session["User"] = userInDb;
                    TempData["Success"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("UserProfile");
                }
            }

            TempData["Error"] = "Cập nhật thất bại. Vui lòng kiểm tra lại thông tin.";
            return View(currentUser);
        }

        // GET: Hiển thị form sửa
        public ActionResult EditProfile()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");

            int id = (int)Session["UserID"];
            var user = db.Users.Find(id);
            return View(user);
        }

        // POST: Lưu thay đổi
        [HttpPost]
        public ActionResult EditProfile(User model)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");

            // Chỉ cập nhật các trường cho phép (tránh user hack sửa Role hoặc Password ở đây)
            int id = (int)Session["UserID"];
            var userInDb = db.Users.Find(id);

            if (userInDb != null)
            {
                userInDb.FullName = model.FullName;
                userInDb.Phone = model.Phone;
                userInDb.Address = model.Address;
                // Không cập nhật Email, Password, Role tại đây

                db.SaveChanges();
                Session["User"] = userInDb; // Cập nhật lại Session
                TempData["UpdateSuccess"] = "Cập nhật thông tin thành công!";
            }

            return RedirectToAction("UserProfile");
        }
    }
}
