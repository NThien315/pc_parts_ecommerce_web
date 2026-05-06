using DOANTKWEB.Models;
using System;
using System.Web.Mvc;

namespace DOANTKWEB.Controllers
{
    public class ReviewController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        [HttpPost]
        public ActionResult Add(int productId, string content, int rating)
        {
            if (Session["User"] == null)
                return RedirectToAction("Login", "Account");

            var user = Session["User"] as User;

            var review = new Review
            {
                ProductID = productId,
                UserID = user.UserID,
                Comment = content,
                Rating = rating,
                ReviewDate = DateTime.Now
            };

            db.Reviews.Add(review);
            db.SaveChanges();

            return RedirectToAction("Details", "Product", new { id = productId });
        }

        public ActionResult Delete(int id)
        {
            var review = db.Reviews.Find(id);
            if (review == null)
                return HttpNotFound();

            // Kiểm tra quyền sở hữu
            // Nếu chưa đăng nhập HOẶC người đăng nhập không phải tác giả bài review
            if (Session["UserID"] == null || (int)Session["UserID"] != review.UserID)
            {
                return RedirectToAction("Login", "Account");
            }

            int productId = review.ProductID;

            db.Reviews.Remove(review);
            db.SaveChanges();

            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }
}
