using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyAmThucNhaTrang.BLL;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.Controllers
{
    public class TaiKhoanController : Controller
    {
        private TaiKhoanBLL _taiKhoanBLL = new TaiKhoanBLL();

        // 1. GIAO DIỆN ĐĂNG NHẬP (GET)
        public ActionResult DangNhap()
        {
            return View();
        }

        // 2. XỬ LÝ ĐĂNG NHẬP (POST)
        [HttpPost]
        public ActionResult DangNhap(string TenDangNhap, string MatKhau)
        {
            var user = _taiKhoanBLL.DangNhap(TenDangNhap, MatKhau);
            if (user != null)
            {
                // Gán Session chuẩn để _LoginPartial.cshtml nhận diện được
                Session["MaTK"] = user.MaTK;
                Session["HoTen"] = user.HoTen;
                Session["LoaiTK"] = user.LoaiTK;

                TempData["Success"] = "Chào mừng " + user.HoTen + " đã quay trở lại!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
            return View();
        }

        // 3. GIAO DIỆN ĐĂNG KÝ (GET)
        public ActionResult DangKy()
        {
            return View();
        }

        // 4. XỬ LÝ ĐĂNG KÝ (POST)
        [HttpPost]
        public ActionResult DangKy(TAIKHOAN tk)
        {
            // Mặc định đăng ký mới là Khách hàng (theo báo cáo của bạn)
            tk.LoaiTK = "KhachHang";

            string result = _taiKhoanBLL.DangKy(tk);
            if (result == "Success")
            {
                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap");
            }

            ViewBag.Error = result;
            return View(tk);
        }

        // 5. ĐĂNG XUẤT
        public ActionResult DangXuat()
        {
            Session.Clear(); // Xóa sạch thông tin đăng nhập
            return RedirectToAction("Index", "Home");
        }

        // 1. GIAO DIỆN TRANG ĐĂNG KÝ ĐỐI TÁC (GET)
        public ActionResult DangKyDoiTac()
        {
            // Nếu người dùng đã đăng nhập rồi thì không cho vào trang đăng ký nữa
            if (Session["MaTK"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 2. XỬ LÝ ĐĂNG KÝ TÀI KHOẢN CHỦ CƠ SỞ (POST)
        [HttpPost]
        public ActionResult DangKyDoiTac(string TenDangNhap, string MatKhau, string HoTen, string Email, string SDT)
        {
            // Bước 1: Khởi tạo đối tượng TAIKHOAN với loại tài khoản ép cứng là ChuCSKD
            TAIKHOAN tkDoiTac = new TAIKHOAN
            {
                TenDangNhap = TenDangNhap,
                MatKhau = MatKhau, // Truyền mật khẩu gốc, hàm DangKy trong BLL sẽ tự lo việc băm SHA-256
                HoTen = HoTen,
                Email = Email,
                SDT = SDT,
                LoaiTK = "ChuCSKD" // Đây là mấu chốt của trang này!
            };

            // Bước 2: Gọi hàm DangKy có sẵn trong BLL của bạn
            // Hàm này sẽ tự động kiểm tra trùng lặp, mã hóa mật khẩu, và set ngày tạo/trạng thái
            string ketQua = _taiKhoanBLL.DangKy(tkDoiTac);

            if (ketQua == "Success")
            {
                TempData["Success"] = "Đăng ký tài khoản Đối tác thành công! Hãy đăng nhập để mở gian hàng.";
                return RedirectToAction("DangNhap");
            }
            else
            {
                // Nếu bị lỗi (như trùng Tên đăng nhập, Email...), trả câu thông báo đó về giao diện
                ViewBag.Error = ketQua;
                return View();
            }
        }
    }
}