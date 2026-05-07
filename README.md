# PCPartsWeb - E-Commerce Website for Computer Components

> Đồ án môn Lập trình Web

Đây là dự án Website Thương mại điện tử chuyên cung cấp linh kiện máy tính, được xây dựng trên nền tảng **C# ASP.NET MVC 5**. Dự án không chỉ cung cấp các chức năng mua sắm cơ bản mà còn tích hợp hệ thống **"Build PC"** cho phép thêm cấu hình tùy chỉnh vào giỏ hàng, cùng với hệ thống quản trị (Admin Dashboard) chặt chẽ.

## Giao diện ứng dụng (Screenshots)
> Trang Chủ
<img width="1240" height="2716" alt="image" src="https://github.com/user-attachments/assets/d0ba97ef-0aa8-497b-a1dd-ff78a405ca5f" />

> Giỏ Hàng
<img width="1240" height="960" alt="image" src="https://github.com/user-attachments/assets/9759a136-bba9-4038-bb79-b117f392bec7" />

> Chi tiết sản phẩm
<img width="1240" height="1532" alt="image" src="https://github.com/user-attachments/assets/4267a28a-ed34-4c34-8df2-99e94198229b" />

## Công nghệ sử dụng (Tech Stack)

### Frontend
* **Ngôn ngữ:** HTML5, CSS3, JavaScript.
* **UI Framework:** `Bootstrap 5.3.8` kết hợp với `jQuery` để thao tác DOM.
* **View Engine:** `Razor (.cshtml)` xử lý Data Binding và render UI động.
* **Libraries:** `SweetAlert2` (Thông báo UI/UX), `PagedList.Mvc` (Phân trang dữ liệu).

### Backend & Database
* **Framework:** C# ASP.NET MVC 5 (.NET Framework 4.7.2).
* **Cơ sở dữ liệu:** SQL Server.
* **ORM:** Entity Framework 6.
* **Bảo mật:** Password Hashing kết hợp với Authorization phân quyền Admin/User qua Session.

## Tính năng nổi bật (Key Features)

### Dành cho Khách hàng (User)
* **Build PC thông minh:** Hỗ trợ chọn cấu hình và thêm **hàng loạt linh kiện cùng lúc** vào giỏ hàng (`AddMultiple` feature).
* **Quản lý Giỏ hàng bền vững:** Giỏ hàng được ánh xạ trực tiếp với tài khoản người dùng và lưu vào Database, không bị mất dữ liệu khi đóng trình duyệt.
* **Lọc & Tìm kiếm:** Lọc linh kiện theo danh mục (Category), thương hiệu (Brand) và từ khóa với kỹ thuật phân trang (Pagination).
* **Quản lý hồ sơ:** Xem lịch sử đơn hàng, cập nhật thông tin cá nhân và thay đổi mật khẩu an toàn.

### Dành cho Quản trị viên (Admin)
* **CRUD Sản phẩm:** Thêm, sửa, xóa linh kiện với chức năng **Upload hình ảnh** an toàn (tự động đổi tên file chống trùng lặp theo thời gian thực).
* **Phân quyền chặt chẽ:** Các tác vụ quản trị được bảo vệ hoàn toàn, yêu cầu xác thực `Role = "Admin"`.
