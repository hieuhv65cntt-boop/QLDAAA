using QuanLyAmThucNhaTrang.BLL;
using QuanLyAmThucNhaTrang.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace QuanLyAmThucNhaTrang.Controllers
{
    public class AdminController : Controller
    {
        private AdminBLL _adminBLL = new AdminBLL();

        // Hàm hỗ trợ kiểm tra quyền Admin
        private bool IsAdmin()
        {
            return Session["LoaiTK"] != null && Session["LoaiTK"].ToString() == "QuanTriVien";
        }

        // ==========================================
        // 1. MÀN HÌNH KIỂM DUYỆT ĐỊA ĐIỂM
        // ==========================================
        public ActionResult KiemDuyetDiaDiem()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            var dsChoDuyet = _adminBLL.LayDanhSachChoDuyet();
            return View(dsChoDuyet);
        }

        // Action Phê Duyệt
        [HttpPost]
        public ActionResult PheDuyet(int maDD)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            if (_adminBLL.PheDuyetDiaDiem(maDD))
            {
                TempData["Success"] = "Đã phê duyệt địa điểm thành công! Địa điểm đã được hiển thị công khai.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi phê duyệt.";
            }
            return RedirectToAction("KiemDuyetDiaDiem");
        }

        // Action Từ Chối
        [HttpPost]
        public ActionResult TuChoi(int maDD, string lyDo)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            if (_adminBLL.TuChoiDiaDiem(maDD, lyDo))
            {
                // Thêm lý do vào thông báo để Admin thấy hệ thống đã ghi nhận
                TempData["Success"] = $"Đã từ chối địa điểm. Lý do: {lyDo}";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi từ chối.";
            }
            return RedirectToAction("KiemDuyetDiaDiem");
        }

        public ActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            // Lấy cục dữ liệu thống kê từ BLL và ném vào ViewBag
            ViewBag.ThongKe = _adminBLL.ThongKeNhanh();

            return View();
        }
        //====================================================================================================
        // 2. MÀN HÌNH XỬ LÝ ĐÁNH GIÁ VI PHẠM (Đã nâng cấp có Bộ Lọc)
        public ActionResult QuanLyDanhGia(int? locSoSao, string tuKhoa, int? page)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            ViewBag.LocSoSao = locSoSao;
            ViewBag.TuKhoa = tuKhoa;

            // Lấy toàn bộ dữ liệu đã lọc
            var dsDanhGia = _adminBLL.LayDanhSachDanhGia(locSoSao, tuKhoa);

            // --- THIẾT LẬP PHÂN TRANG ---
            int pageSize = 8; // Quy định số lượng đánh giá hiển thị trên 1 trang (bạn có thể đổi thành 20, 50 tùy ý)
            int pageNumber = (page ?? 1); // Nếu không có tham số page truyền vào thì mặc định là trang 1

            // Trả về dữ liệu đã được cắt trang thay vì toàn bộ danh sách
            return View(dsDanhGia.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult AnDanhGia(int maDG)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            if (_adminBLL.AnDanhGia(maDG))
                TempData["Success"] = "Đã ẩn đánh giá và cập nhật lại điểm số của quán!";
            else
                TempData["Error"] = "Có lỗi xảy ra khi ẩn đánh giá.";

            return RedirectToAction("QuanLyDanhGia");
        }

        [HttpPost]
        public ActionResult XoaDanhGia(int maDG)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            if (_adminBLL.XoaDanhGia(maDG))
                TempData["Success"] = "Đã xóa vĩnh viễn đánh giá thành công!";
            else
                TempData["Error"] = "Có lỗi xảy ra khi xóa đánh giá.";

            return RedirectToAction("QuanLyDanhGia");
        }

        // 3. THIẾT LẬP DANH MỤC
        // ==========================================
        public ActionResult QuanLyDanhMuc()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");
            return View(_adminBLL.LayDanhSachDanhMuc());
        }

        [HttpPost]
        public ActionResult ThemDanhMuc(DANHMUC dm)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");
            string kq = _adminBLL.ThemDanhMuc(dm);
            if (kq == "Success") TempData["Success"] = "Thêm danh mục thành công!";
            else TempData["Error"] = kq;
            return RedirectToAction("QuanLyDanhMuc");
        }

        [HttpPost]
        public ActionResult SuaDanhMuc(DANHMUC dm)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");
            string kq = _adminBLL.CapNhatDanhMuc(dm);
            if (kq == "Success") TempData["Success"] = "Cập nhật danh mục thành công!";
            else TempData["Error"] = kq;
            return RedirectToAction("QuanLyDanhMuc");
        }

        [HttpPost]
        public ActionResult XoaDanhMuc(int maDM)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");
            if (_adminBLL.XoaDanhMuc(maDM)) TempData["Success"] = "Đã xử lý xóa/ngừng sử dụng danh mục!";
            else TempData["Error"] = "Có lỗi xảy ra khi xóa.";
            return RedirectToAction("QuanLyDanhMuc");
        }


        // 5. KẾT XUẤT BÁO CÁO
        // ==========================================
        public ActionResult BaoCaoDiaDiem(System.DateTime? TuNgay, System.DateTime? DenNgay)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            // Mặc định nếu chưa chọn ngày: Lấy từ đầu năm đến ngày hiện tại
            System.DateTime startDate = TuNgay ?? new System.DateTime(System.DateTime.Now.Year, 1, 1);
            System.DateTime endDate = DenNgay ?? System.DateTime.Now.Date;

            // Truyền ngày đã chọn lại cho View để hiển thị vào thanh công cụ
            ViewBag.TuNgay = startDate.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = endDate.ToString("yyyy-MM-dd");

            var data = _adminBLL.ThongKeDiaDiem(startDate, endDate);
            return View(data);
        }

        public ActionResult BaoCaoTaiKhoan(System.DateTime? TuNgay, System.DateTime? DenNgay)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            System.DateTime startDate = TuNgay ?? new System.DateTime(System.DateTime.Now.Year, 1, 1);
            System.DateTime endDate = DenNgay ?? System.DateTime.Now.Date;

            ViewBag.TuNgay = startDate.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = endDate.ToString("yyyy-MM-dd");

            var data = _adminBLL.ThongKeTaiKhoan(startDate, endDate);
            return View(data);
        }

        public ActionResult BaoCaoDanhGia(System.DateTime? TuNgay, System.DateTime? DenNgay)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "TaiKhoan");

            System.DateTime startDate = TuNgay ?? new System.DateTime(System.DateTime.Now.Year, 1, 1);
            System.DateTime endDate = DenNgay ?? System.DateTime.Now.Date;

            ViewBag.TuNgay = startDate.ToString("yyyy-MM-dd");
            ViewBag.DenNgay = endDate.ToString("yyyy-MM-dd");

            var data = _adminBLL.ThongKeDanhGia(startDate, endDate);
            return View(data);
        }
    }
}