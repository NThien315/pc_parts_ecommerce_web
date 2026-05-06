using DOANTKWEB.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace DOANTKWEB.Controllers
{
    public class CartController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        // Hàm tiện ích: kiểm tra đã đăng nhập chưa
        private bool IsLoggedIn()
        {
            return Session["UserID"] != null;
        }

        private int GetCurrentUserId()
        {
            return (int)Session["UserID"];   // nếu UserID kiểu khác (long) thì đổi cho phù hợp
        }

        private Cart GetCart()
        {
            if (!IsLoggedIn())
                return null;

            int userId = GetCurrentUserId();

            var cart = db.Carts
                .Include(c => c.CartItems.Select(i => i.Product))
                .FirstOrDefault(c => c.UserID == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserID = userId,
                    DateCreated = DateTime.Now,
                    CartItems = new List<CartItem>()
                };

                db.Carts.Add(cart);
                db.SaveChanges();
            }
            else if (cart.CartItems == null)
            {
                cart.CartItems = new List<CartItem>();
            }

            return cart;
        }

        // ===== XEM GIỎ HÀNG =====
        public ActionResult Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            var cart = GetCart();
            return View(cart);
        }

        // ===== THÊM SẢN PHẨM VÀO GIỎ =====
        public ActionResult Add(int productId, int quantity = 1, string returnUrl = null)
        {
            if (!IsLoggedIn())
            {
                TempData["LoginError"] = "Vui lòng đăng nhập trước khi thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            int userId = GetCurrentUserId();

            var cart = db.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserID == userId);

            // Nếu chưa có giỏ → tạo mới
            if (cart == null)
            {
                cart = new Cart
                {
                    UserID = userId,
                    DateCreated = DateTime.Now
                };

                db.Carts.Add(cart);
                db.SaveChanges();
            }

            TempData["AddCartSuccess"] = "Đã thêm sản phẩm vào giỏ hàng thành công!";

            // Tìm item trong giỏ
            var item = db.CartItems
                .FirstOrDefault(i => i.CartID == cart.CartID && i.ProductID == productId);

            if (item == null)
            {
                item = new CartItem
                {
                    CartID = cart.CartID,
                    ProductID = productId,
                    Quantity = quantity
                };

                db.CartItems.Add(item);
            }
            else
            {
                item.Quantity += quantity;
            }

            db.SaveChanges();

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // ===== XÓA 1 DÒNG TRONG GIỎ =====
        public ActionResult Remove(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            int userId = GetCurrentUserId();

            var item = db.CartItems
                        .Include(i => i.Cart)
                        .FirstOrDefault(i => i.CartItemID == id && i.Cart.UserID == userId);

            if (item != null)
            {
                db.CartItems.Remove(item);
                db.SaveChanges();

                TempData["RemoveCartSuccess"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            return RedirectToAction("Index");
        }

        // ===== CẬP NHẬT SỐ LƯỢNG =====
        [HttpPost]
        public ActionResult Update(int itemId, int quantity)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            // Nếu user nhập <= 0, tự động ép về 1
            if (quantity <= 0) quantity = 1;

            int userId = GetCurrentUserId();
            var item = db.CartItems.Include(i => i.Cart)
                        .FirstOrDefault(i => i.CartItemID == itemId && i.Cart.UserID == userId);

            if (item != null)
            {
                // Kiểm tra tồn kho lần nữa
                var product = db.Products.Find(item.ProductID);
                if (product != null && quantity > product.StockQuantity)
                {
                    // Nếu mua quá kho -> Set bằng số tối đa kho có
                    quantity = product.StockQuantity;
                }

                item.Quantity = quantity;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // Action này chỉ trả về con số số lượng, dùng để nhúng vào Layout
        [ChildActionOnly] // Chỉ cho phép gọi từ View
        public ActionResult CartBadge()
        {
            if (Session["UserID"] == null)
            {
                return Content("0");
            }

            int userId = (int)Session["UserID"];

            // Đếm tổng số lượng sản phẩm trong giỏ của User này
            var totalQty = db.CartItems
                             .Where(i => i.Cart.UserID == userId)
                             .Sum(i => (int?)i.Quantity) ?? 0;

            return Content(totalQty.ToString());
        }

        // Action thêm nhiều sản phẩm cùng lúc (Build PC)
        public ActionResult AddMultiple(string productIds, string quantities)
        {
            // 1. KIỂM TRA ĐĂNG NHẬP
            // Nếu chưa đăng nhập mà cố thêm vào giỏ -> Về trang Login
            if (Session["UserID"] == null)
            {
                TempData["LoginError"] = "Vui lòng đăng nhập để thêm cấu hình vào giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            // 2. KIỂM TRA DỮ LIỆU ĐẦU VÀO
            if (string.IsNullOrEmpty(productIds))
                return RedirectToAction("BuildPCView", "Home");

            // Xử lý danh sách ID
            List<int> ids;
            try
            {
                ids = productIds.Split(',').Select(int.Parse).ToList();
            }
            catch
            {
                return RedirectToAction("BuildPCView", "Home"); // Lỗi format ID
            }

            // Xử lý danh sách Số lượng (Chống lỗi Null)
            List<int> qtys = new List<int>();
            if (!string.IsNullOrEmpty(quantities))
            {
                try
                {
                    qtys = quantities.Split(',').Select(int.Parse).ToList();
                }
                catch { /* Bỏ qua nếu lỗi format số lượng */ }
            }

            // 3. XỬ LÝ GIỎ HÀNG
            int userId = (int)Session["UserID"];

            // Include CartItems để kiểm tra trùng
            var cart = db.Carts.Include("CartItems")
                               .FirstOrDefault(c => c.UserID == userId);

            // Nếu chưa có giỏ hàng -> Tạo mới
            if (cart == null)
            {
                cart = new Cart
                {
                    UserID = userId,
                    DateCreated = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                db.Carts.Add(cart);
                db.SaveChanges();
            }

            // Đảm bảo CartItems không null
            if (cart.CartItems == null) cart.CartItems = new List<CartItem>();

            // 4. DUYỆT VÀ THÊM SẢN PHẨM
            for (int i = 0; i < ids.Count; i++)
            {
                int pId = ids[i];
                int qty = (qtys.Count > i) ? qtys[i] : 1; // Nếu thiếu số lượng thì mặc định là 1

                // Kiểm tra sản phẩm có tồn tại trong DB không (Tránh lỗi khóa ngoại)
                var productExists = db.Products.Any(p => p.ProductID == pId);
                if (!productExists) continue;

                var item = db.CartItems.FirstOrDefault(x => x.CartID == cart.CartID && x.ProductID == pId);

                if (item == null)
                {
                    // Thêm mới
                    item = new CartItem
                    {
                        CartID = cart.CartID,
                        ProductID = pId,
                        Quantity = qty
                    };
                    db.CartItems.Add(item);
                }
                else
                {
                    // Cộng dồn
                    item.Quantity += qty;
                }
            }

            db.SaveChanges();

            TempData["AddCartSuccess"] = "Đã thêm toàn bộ cấu hình vào giỏ hàng!";
            return RedirectToAction("Index");
        }
    }
}