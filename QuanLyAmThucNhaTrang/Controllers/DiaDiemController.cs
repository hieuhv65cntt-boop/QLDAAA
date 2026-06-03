using QuanLyAmThucNhaTrang.BLL;
using QuanLyAmThucNhaTrang.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyAmThucNhaTrang.Controllers
{
    public class DiaDiemController : Controller
    {
        private YeuThichBLL _yeuThichBLL = new YeuThichBLL();
        private DiaDiemBLL _diaDiemBLL = new DiaDiemBLL();
        private DanhGiaBLL _danhGiaBLL = new DanhGiaBLL();
        private KhuyenMaiBLL _khuyenMaiBLL = new KhuyenMaiBLL();

        // 1. GIAO DIỆN BẢN ĐỒ (GET)
        public ActionResult BanDo()
        {
            // Truyền dữ liệu danh mục và khu vực sang View để làm bộ lọc
            ViewBag.DanhMucList = _diaDiemBLL.LayTatCaDanhMuc();
            ViewBag.KhuVucList = _diaDiemBLL.LayTatCaKhuVuc();
            return View();
        }

        // 2. API TRẢ VỀ DỮ LIỆU JSON CHO BẢN ĐỒ (Dùng AJAX gọi ngầm)
        [HttpGet]
        public JsonResult LayDuLieuBanDo(string tuKhoa, int? maDM, int? maKV)
        {
            // Tái sử dụng hàm tìm kiếm đã viết
            var dsDiaDiem = _diaDiemBLL.TimKiemVaLoc(tuKhoa, maDM, maKV);

            // Bọc dữ liệu lại, CHỈ LẤY những cột cần thiết để đưa lên bản đồ
            // (Tránh lỗi Circular Reference - Vòng lặp vô tận của Entity Framework khi parse JSON)
            var result = dsDiaDiem.Select(d => new {
                MaDD = d.MaDD,
                TenDD = d.TenDD,
                ViDo = d.ViDo,
                KinhDo = d.KinhDo,
                DiaChi = d.DiaChiChiTiet,
                Diem = d.DiemDanhGiaTB,
                TenDM = d.DANHMUC.TenDM
            }).ToList();

            // Trả về định dạng JSON
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ThemDanhGia(int MaDD, int SoSao, string NoiDung)
        {
            // 1. Kiểm tra đăng nhập (Chặn những ai cố tình gọi URL khi chưa đăng nhập)
            if (Session["MaTK"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để viết đánh giá!";
                return RedirectToAction("ChiTiet", new { id = MaDD });
            }

            int maTK = (int)Session["MaTK"];

            // 2. Gọi BLL xử lý
            string ketQua = _danhGiaBLL.GuiDanhGia(maTK, MaDD, SoSao, NoiDung);

            if (ketQua == "Success")
            {
                TempData["Success"] = "Cảm ơn bạn đã chia sẻ trải nghiệm tuyệt vời này!";
            }
            else
            {
                TempData["Error"] = ketQua; // Báo lỗi nếu đã đánh giá rồi
            }

            // 3. Xong xuôi thì load lại trang chi tiết đó
            return RedirectToAction("ChiTiet", new { id = MaDD });
        }

        // GET: DiaDiem/TimKiem
        public ActionResult TimKiem(string tuKhoa, int? maDM, int? maKV)
        {
            // 1. Lấy dữ liệu nạp vào các bộ lọc trên giao diện
            ViewBag.DanhMucList = _diaDiemBLL.LayTatCaDanhMuc();
            ViewBag.KhuVucList = _diaDiemBLL.LayTatCaKhuVuc();

            // 2. Giữ lại trạng thái người dùng đã chọn để hiển thị lại trên Form sau khi tải lại trang
            ViewBag.TuKhoaHienTai = tuKhoa;
            ViewBag.MaDMHienTai = maDM;
            ViewBag.MaKVHienTai = maKV;

            // 3. Thực hiện tìm kiếm dữ liệu
            var kếtQuả = _diaDiemBLL.TimKiemVaLoc(tuKhoa, maDM, maKV);

            return View(kếtQuả);
        }

        // GET: DiaDiem/ChiTiet/5
        public ActionResult ChiTiet(int id)
        {
            // 1. Lấy thông tin địa điểm từ BLL
            var dd = _diaDiemBLL.LayChiTietDiaDiem(id);
            if (dd == null) return HttpNotFound("Không tìm thấy dữ liệu.");

            // 2. KHỞI TẠO BỘ NHẬN DIỆN QUYỀN (VIP PASS)
            // Kiểm tra xem người đang truy cập có phải là Admin không?
            bool isAdmin = Session["LoaiTK"] != null && Session["LoaiTK"].ToString() == "QuanTriVien";

            // Kiểm tra xem người đang truy cập có phải là Chủ của chính quán này không?
            bool isOwner = Session["MaTK"] != null && (int)Session["MaTK"] == dd.MaTK;

            // 3. LOGIC CHẶN HIỂN THỊ
            // Nếu quán đang không hoạt động bình thường, và người xem lại KHÔNG PHẢI admin, CŨNG KHÔNG PHẢI chủ quán
            // -> Lúc này mới chắc chắn là Khách vãng lai -> Chặn lại báo lỗi 404
            if (dd.TrangThai.Trim() != "DangHoatDong" && dd.TrangThai.Trim() != "TamNgung" && !isAdmin && !isOwner)
            {
                return HttpNotFound("Cơ sở này hiện không hoạt động hoặc đang chờ cấp phép.");
            }

            // Nếu qua được ải trên (tức là quán đang Hoạt động, HOẶC người đang xem là Admin/Chủ quán)
            // -> Lấy thêm dữ liệu liên quan (như bình luận, đánh giá...) và hiển thị View
            return View(dd);
        }

        // [POST] Xử lý đăng ký quán mới - Nhận nhiều file ảnh cùng lúc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKyDiaDiem(DIADIEM dd, IEnumerable<HttpPostedFileBase> hinhAnhs)
        {
            // 1. Kiểm tra điều kiện đăng nhập và quyền Chủ quán
            if (Session["MaTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maTK = Convert.ToInt32(Session["MaTK"]);
            dd.MaTK = maTK;

            // 2. Chuyển đổi danh sách file hợp lệ (loại bỏ các file rỗng)
            var danhSachFile = hinhAnhs != null
                ? hinhAnhs.Where(f => f != null && f.ContentLength > 0).ToList()
                : new List<HttpPostedFileBase>();

            // 3. RÀNG BUỘC QĐ5: Kiểm tra số lượng ảnh tối thiểu (ít nhất 2 ảnh mặt tiền)
            if (danhSachFile.Count < 2)
            {
                ModelState.AddModelError("", "Biểu mẫu bắt buộc phải tải lên ít nhất 2 hình ảnh mặt tiền quán để xác minh!");
                // Nạp lại các danh mục, khu vực cho Dropdown trước khi trả về View hiển thị lỗi
                ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM");
                ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV");
                return View(dd);
            }

            if (ModelState.IsValid)
            {
                // 4. Lưu thông tin địa điểm trước để sinh ra MaDD (Khóa chính tự tăng)
                int maDDMoi = _diaDiemBLL.ThemDiaDiemMoi(dd);

                if (maDDMoi > 0)
                {
                    // 5. Vòng lặp lưu từng file ảnh vào thư mục và ghi vào CSDL
                    int thuTu = 1;
                    foreach (var file in danhSachFile)
                    {
                        // Tạo tên file duy nhất bằng GUID để tránh trùng lặp tệp tin trên máy chủ
                        string tenFile = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string duongDanThuMuc = Server.MapPath("~/images/uploads/");

                        // Đảm bảo thư mục tồn tại
                        if (!Directory.Exists(duongDanThuMuc)) Directory.CreateDirectory(duongDanThuMuc);

                        string duongDanVatLy = Path.Combine(duongDanThuMuc, tenFile);
                        file.SaveAs(duongDanVatLy); // Lưu file vào ổ cứng server

                        // Đường dẫn tương đối lưu xuống database để hiển thị trên web
                        string duongDanDb = "/images/uploads/" + tenFile;

                        // Gọi BLL ghi vào bảng HINHANH, gán loại hình là 'MatTien' cho các ảnh xác minh ban đầu
                        _diaDiemBLL.ThemHinhAnh(maDDMoi, duongDanDb, "MatTien");
                        thuTu++;
                    }

                    TempData["Success"] = "Gửi yêu cầu đăng ký địa điểm thành công! Vui lòng chờ Ban quản trị kiểm duyệt.";
                    return RedirectToAction("QuanLyGianHang", "ChuCSKD");
                }
            }

            return View(dd);
        }

        // [POST] Xử lý hủy yêu cầu phê duyệt từ phía Chủ quán
        [HttpPost]
        public ActionResult XoaYeuCau(int id)
        {
            if (Session["MaTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maTK = Convert.ToInt32(Session["MaTK"]);

            if (_diaDiemBLL.XoaYeuCauDangKy(id, maTK))
            {
                TempData["Success"] = "Đã hủy bỏ và xóa hoàn toàn yêu cầu phê duyệt địa điểm thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể xóa yêu cầu này (Có thể địa điểm đã được phê duyệt hoặc không thuộc quyền sở hữu của bạn).";
            }

            return RedirectToAction("QuanLyGianHang", "ChuCSKD");
        }
    }
}