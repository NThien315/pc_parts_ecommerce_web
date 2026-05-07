using PCPartsWeb.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace PCPartsWeb.Controllers 
{
    public class OrderController : Controller
    {
        private SaleDbContext db = new SaleDbContext();



        // ===== TẠO ĐƠN HÀNG =====
        public ActionResult Checkout()
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserID"];

            var cart = db.Carts.Include("CartItems.Product").FirstOrDefault(c => c.UserID == userId);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            // === KIỂM TRA TỒN KHO TRƯỚC ===
            foreach (var item in cart.CartItems)
            {
                var productInDb = db.Products.Find(item.ProductID);
                if (productInDb == null) continue;

                // Nếu số lượng mua lớn hơn số lượng trong kho
                if (item.Quantity > productInDb.StockQuantity)
                {
                    TempData["Error"] = $"Sản phẩm '{productInDb.ProductName}' chỉ còn {productInDb.StockQuantity} cái. Vui lòng cập nhật lại giỏ hàng.";
                    return RedirectToAction("Index", "Cart"); // Đẩy về trang giỏ hàng để khách sửa
                }
            }

            // === TÍNH TOÁN & TẠO ĐƠN (Logic cũ) ===
            decimal total = 0;
            foreach (var i in cart.CartItems)
            {
                decimal unitPrice = (i.Product.DiscountPrice.HasValue && i.Product.DiscountPrice < i.Product.Price)
                                    ? i.Product.DiscountPrice.Value : i.Product.Price;
                total += unitPrice * i.Quantity;
            }

            var user = db.Users.Find(userId);
            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                Status = "Pending",
                PaymentMethod = "COD",
                Address = user.Address
            };

            db.Orders.Add(order);
            db.SaveChanges(); // Lưu để lấy OrderID

            // === LƯU CHI TIẾT & TRỪ KHO ===
            foreach (var item in cart.CartItems)
            {
                decimal unitPrice = (item.Product.DiscountPrice.HasValue && item.Product.DiscountPrice < item.Product.Price)
                                    ? item.Product.DiscountPrice.Value : item.Product.Price;

                db.OrderItems.Add(new OrderItem
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = unitPrice
                });

                // Trừ tồn kho
                var productToUpdate = db.Products.Find(item.ProductID);
                if (productToUpdate != null)
                {
                    productToUpdate.StockQuantity -= item.Quantity;
                }
            }

            // Xóa giỏ hàng & Lưu DB
            db.CartItems.RemoveRange(cart.CartItems);
            db.SaveChanges();

            TempData["OrderSuccess"] = true;
            // TempData["NewOrderID"] = order.OrderID;

            TempData["Success"] = "Đặt hàng thành công! Mã đơn: #" + order.OrderID;

            return RedirectToAction("UserProfile", "Account");
        }

        // ===== CHI TIẾT ĐƠN HÀNG =====
        public ActionResult Details(int id)
        {
            // Lấy User và Role hiện tại
            var user = Session["User"] as PCPartsWeb.Models.User;
            if (user == null) return RedirectToAction("Login", "Account");

            // Tìm đơn hàng kèm chi tiết
            var order = db.Orders.Include("OrderItems.Product")
                                 .FirstOrDefault(o => o.OrderID == id);

            if (order == null) return HttpNotFound();

            // ★ LOGIC QUAN TRỌNG: Cho phép xem nếu là (Chính chủ) HOẶC (Là Admin)
            if (order.UserID != user.UserID && user.Role != "Admin")
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden, "Bạn không có quyền xem đơn này");
            }

            return View(order);
        }

        // --- PHẦN DÀNH RIÊNG CHO ADMIN ---

        // GET: Order/Management (Quản lý đơn hàng - Có Tìm kiếm & Lọc)
        public ActionResult Management(string search, string status, int? page)
        {
            // 1. Chặn quyền
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            var query = db.Orders.Include("User").AsQueryable();

            // 2. Tìm kiếm (Theo Mã đơn Hoặc Tên khách)
            if (!string.IsNullOrEmpty(search))
            {
                // Thử parse sang số để tìm theo ID, nếu không được thì tìm theo tên
                if (int.TryParse(search, out int orderId))
                {
                    query = query.Where(o => o.OrderID == orderId);
                }
                else
                {
                    query = query.Where(o => o.User.FullName.Contains(search));
                }
            }

            // 3. Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Lưu lại giá trị filter để hiện lại trên View
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;

            // 4. Sắp xếp đơn mới nhất lên đầu
            var orders = query.OrderByDescending(o => o.OrderDate).ToList();

            return View(orders);
        }

        // POST: Order/UpdateStatus (Cập nhật trạng thái + Hoàn kho)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status)
        {
            if (Session["Role"] as string != "Admin") return RedirectToAction("Login", "Account");

            var order = db.Orders.Include("OrderItems").FirstOrDefault(o => o.OrderID == id);
            if (order != null)
            {
                // LOGIC HOÀN KHO
                if (order.Status != "Cancelled" && status == "Cancelled")
                {
                    foreach (var item in order.OrderItems)
                    {
                        var p = db.Products.Find(item.ProductID);
                        if (p != null) p.StockQuantity += item.Quantity;
                    }
                }
                else if (order.Status == "Cancelled" && status != "Cancelled")
                {
                    foreach (var item in order.OrderItems)
                    {
                        var p = db.Products.Find(item.ProductID);
                        if (p != null)
                        {
                            if (p.StockQuantity < item.Quantity)
                            {
                                TempData["Error"] = $"Sản phẩm {p.ProductName} không đủ hàng để khôi phục đơn!";
                                return RedirectToAction("Management");
                            }
                            p.StockQuantity -= item.Quantity;
                        }
                    }
                }

                order.Status = status;
                db.SaveChanges();
                TempData["Success"] = $"Cập nhật đơn hàng #{id} thành công!";
            }
            return RedirectToAction("Management");
        }
    }
}
