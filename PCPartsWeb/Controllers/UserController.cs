using PCPartsWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PCPartsWeb.Controllers
{
    public class UserController : Controller
    {
        private SaleDbContext db = new SaleDbContext();

        // Admin security
        private bool IsAdmin()
        {
            return Session["Role"] != null && Session["Role"].ToString() == "Admin";
        }

        public ActionResult Index()
        {
            if (!IsAdmin()) return HttpNotFound();

            return View(db.Users.ToList());
        }

        public ActionResult SetRole(int id, string role)
        {
            if (!IsAdmin()) return HttpNotFound();

            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            user.Role = role;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (!IsAdmin()) return HttpNotFound();

            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            db.Users.Remove(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}