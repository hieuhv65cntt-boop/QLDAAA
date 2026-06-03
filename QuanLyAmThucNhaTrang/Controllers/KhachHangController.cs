using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuanLyAmThucNhaTrang.BLL;

namespace QuanLyAmThucNhaTrang.Controllers
{
    public class KhachHangController : Controller
    {
        private YeuThichBLL _yeuThichBLL = new YeuThichBLL();
        private DanhGiaBLL _danhGiaBLL = new DanhGiaBLL();
        private TaiKhoanBLL _taiKhoanBLL = new TaiKhoanBLL();

        // 1. GIAO DIỆN TRANG CÁ NHÂN (GET)
        public ActionResult TrangCaNhan()
        {
            if (Session["MaTK"] == null) return RedirectToAction("DangNhap", "TaiKhoan");

            int maTK = (int)Session["MaTK"];
            var user = _taiKhoanBLL.LayThongTinAcount(maTK);
            return View(user);
        }

        // 2. XỬ LÝ CẬP NHẬT THÔNG TIN HỒ SƠ (POST)
        [HttpPost]
        public ActionResult CapNhatHoSo(string HoTen, string Email, string SDT)
        {
            if (Session["MaTK"] == null) return RedirectToAction("DangNhap", "TaiKhoan");

            int maTK = (int)Session["MaTK"];
            var tk = _taiKhoanBLL.LayThongTinAcount(maTK);

            tk.HoTen = HoTen;
            tk.Email = Email;
            tk.SDT = SDT;

            if (_taiKhoanBLL.CapNhatHoSo(tk))
            {
                Session["HoTen"] = tk.HoTen; // Cập nhật lại tên hiển thị trên Header ngay lập tức
                TempData["Success"] = "Cập nhật hồ sơ cá nhân thành công!";
            }
            else
            {
                TempData["Error"] = "Cập nhật thất bại. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction("TrangCaNhan");
        }

        // 3. XỬ LÝ ĐỔI MẬT KHẨU (POST)
        [HttpPost]
        public ActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi)
        {
            if (Session["MaTK"] == null) return RedirectToAction("DangNhap", "TaiKhoan");

            int maTK = (int)Session["MaTK"];
            string ketQua = _taiKhoanBLL.DoiMatKhau(maTK, MatKhauCu, MatKhauMoi);

            if (ketQua == "Success")
            {
                TempData["Success"] = "Đổi mật khẩu thành công! Hãy ghi nhớ mật khẩu mới.";
            }
            else
            {
                TempData["Error"] = ketQua;
            }

            return RedirectToAction("TrangCaNhan");
        }

        public ActionResult LichSuDanhGia()
        {
            // Kiểm tra bảo mật xem người dùng đã đăng nhập chưa
            if (Session["MaTK"] == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maTK = (int)Session["MaTK"];

            // Gọi BLL lấy danh sách lịch sử đánh giá
            var dsDanhGia = _danhGiaBLL.LayLichSuDanhGiaTheoUser(maTK);

            return View(dsDanhGia);
        }

        // 1. API XỬ LÝ BẬT/TẮT YÊU THÍCH (GỌI QUA AJAX)
        [HttpPost]
        public JsonResult ToggleYeuThich(int maDD)
        {
            // Kiểm tra xem đã đăng nhập chưa
            if (Session["MaTK"] == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            int maTK = (int)Session["MaTK"];

            // Gọi tầng nghiệp vụ xử lý thêm/xóa ngầm
            string ketQua = _yeuThichBLL.XulyYeuThich(maTK, maDD);

            if (ketQua == "Loi")
            {
                return Json(new { success = false, message = "Có lỗi hệ thống xảy ra." });
            }

            // Trả về kết quả thành công kèm trạng thái hiện tại (DaLuu hoặc DaXoa)
            return Json(new { success = true, status = ketQua });
        }

        // 2. TRANG DANH SÁCH ĐỊA ĐIỂM ĐÃ LƯU
        public ActionResult YeuThich()
        {
            if (Session["MaTK"] == null)
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maTK = (int)Session["MaTK"];
            var dsYeuThich = _yeuThichBLL.LayDanhSachYeuThich(maTK);

            return View(dsYeuThich);
        }
    }
}