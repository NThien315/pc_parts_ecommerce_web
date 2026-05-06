namespace DOANTKWEB.Migrations
{
    using DOANTKWEB.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DOANTKWEB.Models.SaleDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(DOANTKWEB.Models.SaleDbContext context)
        {
            // CATEGORY
            context.Categories.AddOrUpdate(
                c => c.CategoryID,
                new Category { CategoryID = 1, CategoryName = "PC Gaming" },
                new Category { CategoryID = 2, CategoryName = "PC Văn Phòng" },
                new Category { CategoryID = 3, CategoryName = "PC Đồ Hoạ" },
                new Category { CategoryID = 4, CategoryName = "Laptop" },
                new Category { CategoryID = 5, CategoryName = "CPU", ParentID = 4 }
            );

            // BRAND
            context.Brands.AddOrUpdate(
                b => b.BrandID,
                new Brand { BrandID = 1, BrandName = "ASUS" },
                new Brand { BrandID = 2, BrandName = "MSI" },
                new Brand { BrandID = 3, BrandName = "Lenovo" },
                new Brand { BrandID = 4, BrandName = "Gigabyte" }
            );

            // USER (Admin)
            context.Users.AddOrUpdate(
                u => u.UserID,
                new User
                {
                    UserID = 1,
                    FullName = "Admin",
                    Email = "admin@gmail.com",
                    PasswordHash = "123456", // sau sẽ mã hoá
                    Role = "Admin"
                }
            );

            // PRODUCT
            context.Products.AddOrUpdate(
                p => p.ProductID,
                new Product
                {
                    ProductID = 1,
                    ProductName = "PC Gaming Ryzen 5 RTX 3060",
                    CategoryID = 1,
                    BrandID = 1,
                    CPU = "Ryzen 5 5600",
                    GPU = "RTX 3060",
                    RAM = "16GB",
                    Storage = "SSD 512GB",
                    PSU = "550W",
                    Mainboard = "B550",
                    Case = "Xigmatek",
                    Price = 19900000,
                    DiscountPrice = 17900000,
                    StockQuantity = 10,
                    WarrantyMonths = 24,
                    Images = "pc1.jpg"
                }
            );
        }
    }
}
