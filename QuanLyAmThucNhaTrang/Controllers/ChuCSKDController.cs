using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using QuanLyAmThucNhaTrang.BLL;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.Controllers
{
    public class ChuCSKDController : Controller
    {
        private TaiKhoanBLL _taiKhoanBLL = new TaiKhoanBLL();
        private DiaDiemBLL _diaDiemBLL = new DiaDiemBLL();
        private KhuyenMaiBLL _khuyenMaiBLL = new KhuyenMaiBLL();
        private PhanHoiBLL _phanHoiBLL = new PhanHoiBLL();

        // 1. GIAO DIỆN FORM ĐĂNG KÝ (GET)
        public ActionResult ThemDiaDiem()
        {
            // Bảo mật: Kiểm tra đúng quyền Chủ cơ sở mới cho vào
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            // Truyền danh sách Danh mục và Khu vực lên Form (Dropdown)
            ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM");
            ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV");

            return View();
        }

        // 2. XỬ LÝ LƯU DỮ LIỆU VÀ FILE ẢNH (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Đã cập nhật tham số để hứng thêm anhKhongGian và anhThucDon từ form
        public ActionResult ThemDiaDiem(DIADIEM dd, IEnumerable<HttpPostedFileBase> hinhAnhs, IEnumerable<HttpPostedFileBase> anhKhongGian, IEnumerable<HttpPostedFileBase> anhThucDon)
        {
            if (Session["MaTK"] == null) return RedirectToAction("DangNhap", "TaiKhoan");

            int maTK = Convert.ToInt32(Session["MaTK"]);
            dd.MaTK = maTK;

            // 1. Kiểm tra Ràng buộc Ảnh Mặt Tiền (Bắt buộc >= 2)
            var danhSachMatTien = hinhAnhs != null
                ? hinhAnhs.Where(f => f != null && f.ContentLength > 0).ToList()
                : new List<HttpPostedFileBase>();

            if (danhSachMatTien.Count < 2)
            {
                ViewBag.Error = "Biểu mẫu bắt buộc phải tải lên ít nhất 2 hình ảnh mặt tiền quán để xác minh!";
                ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM", dd.MaDM);
                ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV", dd.MaKV);
                return View(dd);
            }

            if (ModelState.IsValid)
            {
                // 2. Lưu thông tin cơ bản để sinh ra MaDD
                int maDDMoi = _diaDiemBLL.ThemDiaDiemMoi(dd);

                if (maDDMoi > 0)
                {
                    string duongDanThuMuc = Server.MapPath("~/images/uploads/");
                    if (!Directory.Exists(duongDanThuMuc))
                        Directory.CreateDirectory(duongDanThuMuc);

                    // 3.1 Lưu tập Ảnh Mặt Tiền
                    foreach (var file in danhSachMatTien)
                    {
                        string tenFile = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        file.SaveAs(Path.Combine(duongDanThuMuc, tenFile));
                        _diaDiemBLL.ThemHinhAnh(maDDMoi, "/images/uploads/" + tenFile, "MatTien");
                    }

                    // 3.2 Lưu tập Ảnh Không Gian (Nếu có tải lên)
                    if (anhKhongGian != null)
                    {
                        foreach (var file in anhKhongGian.Where(f => f != null && f.ContentLength > 0))
                        {
                            string tenFile = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            file.SaveAs(Path.Combine(duongDanThuMuc, tenFile));
                            _diaDiemBLL.ThemHinhAnh(maDDMoi, "/images/uploads/" + tenFile, "KhongGian");
                        }
                    }

                    // 3.3 Lưu tập Ảnh Thực Đơn (Nếu có tải lên)
                    if (anhThucDon != null)
                    {
                        foreach (var file in anhThucDon.Where(f => f != null && f.ContentLength > 0))
                        {
                            string tenFile = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            file.SaveAs(Path.Combine(duongDanThuMuc, tenFile));
                            _diaDiemBLL.ThemHinhAnh(maDDMoi, "/images/uploads/" + tenFile, "ThucDon");
                        }
                    }

                    TempData["Success"] = "Gửi yêu cầu đăng ký địa điểm thành công! Vui lòng chờ kiểm duyệt.";
                    return RedirectToAction("QuanLyGianHang");
                }
            }

            ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM", dd.MaDM);
            ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV", dd.MaKV);
            return View(dd);
        }

        // 3. TRANG DANH SÁCH GIAN HÀNG CỦA TÔI (GET)
        public ActionResult QuanLyGianHang()
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD")
            {
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int maTK = (int)Session["MaTK"];
            var dsGianHang = _diaDiemBLL.LayDanhSachTheoChuQuan(maTK);
            return View(dsGianHang);
        }

        // 4. GIAO DIỆN SỬA ĐỊA ĐIỂM (GET)
        public ActionResult SuaDiaDiem(int id)
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");

            var diaDiem = _diaDiemBLL.LayChiTietDiaDiem(id); // Dùng lại hàm lấy chi tiết có sẵn của bạn

            // Bảo mật tối cao: Tránh trường hợp chủ quán dùng URL sửa quán của người khác
            if (diaDiem == null || diaDiem.MaTK != (int)Session["MaTK"])
            {
                return HttpNotFound();
            }

            ViewBag.MaDM = new SelectList(_diaDiemBLL.LayTatCaDanhMuc(), "MaDM", "TenDM", diaDiem.MaDM);
            ViewBag.MaKV = new SelectList(_diaDiemBLL.LayTatCaKhuVuc(), "MaKV", "TenKV", diaDiem.MaKV);

            return View(diaDiem);
        }

        // 5. XỬ LÝ LƯU THÔNG TIN SỬA ĐỔI (POST) - CHUẨN HÓA DRAFT PATTERN
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult SuaDiaDiem(DIADIEM dd, IEnumerable<HttpPostedFileBase> anhKhongGian, IEnumerable<HttpPostedFileBase> anhThucDon, List<int> anhCanXoa)
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");

            // Lấy trước dữ liệu quán cũ để tí nữa copy ảnh
            var quanCu = _diaDiemBLL.LayChiTietDiaDiem(dd.MaDD);

            // 1. Gọi BLL để tạo bản nháp và HỨNG LẤY ID BẢN NHÁP
            int maDD_ThucTe = _diaDiemBLL.CapNhatGianHang(dd);

            if (maDD_ThucTe > 0)
            {
                // 2. LOGIC SAO CHÉP HÌNH ẢNH THÔNG MINH
                if (maDD_ThucTe != dd.MaDD)
                {
                    // NẾU LÀ TẠO BẢN NHÁP MỚI: Copy toàn bộ ảnh từ quán cũ sang bản nháp, 
                    // CHỪ những ảnh mà chủ quán vừa tick chọn xóa.
                    if (quanCu != null && quanCu.HINHANH != null)
                    {
                        foreach (var anh in quanCu.HINHANH)
                        {
                            if (anhCanXoa == null || !anhCanXoa.Contains(anh.MaHA))
                            {
                                // Lưu ý: Copy ảnh sang ID mới (maDD_ThucTe)
                                _diaDiemBLL.ThemHinhAnh(maDD_ThucTe, anh.DuongDan, anh.LoaiHinhAnh);
                            }
                        }
                    }
                }
                else
                {
                    // NẾU ĐANG SỬA TRÊN 1 BẢN NHÁP CÓ SẴN: Thoải mái xóa thẳng ảnh
                    if (anhCanXoa != null && anhCanXoa.Any())
                    {
                        foreach (int maHA in anhCanXoa) _diaDiemBLL.XoaHinhAnh(maHA);
                    }
                }

                // 3. LƯU ẢNH MỚI TẢI LÊN (VÀO ĐÚNG ID BẢN NHÁP - maDD_ThucTe)
                string duongDanThuMuc = Server.MapPath("~/images/uploads/");
                if (!System.IO.Directory.Exists(duongDanThuMuc)) System.IO.Directory.CreateDirectory(duongDanThuMuc);

                if (anhKhongGian != null)
                {
                    foreach (var file in anhKhongGian.Where(f => f != null && f.ContentLength > 0))
                    {
                        string tenFile = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        file.SaveAs(System.IO.Path.Combine(duongDanThuMuc, tenFile));
                        _diaDiemBLL.ThemHinhAnh(maDD_ThucTe, "/images/uploads/" + tenFile, "KhongGian");
                    }
                }

                if (anhThucDon != null)
                {
                    foreach (var file in anhThucDon.Where(f => f != null && f.ContentLength > 0))
                    {
                        string tenFile = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        file.SaveAs(System.IO.Path.Combine(duongDanThuMuc, tenFile));
                        _diaDiemBLL.ThemHinhAnh(maDD_ThucTe, "/images/uploads/" + tenFile, "ThucDon");
                    }
                }

                TempData["Success"] = "Đã cập nhật thông tin và hình ảnh! Quán đang chờ Admin kiểm duyệt.";
                return RedirectToAction("QuanLyGianHang");
            }

            ViewBag.Error = "Cập nhật thất bại. Vui lòng kiểm tra dữ liệu.";
            return View(dd);
        }
        public ActionResult TrangCaNhan()
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");
            int maTK = (int)Session["MaTK"];
            var user = _taiKhoanBLL.LayThongTinAcount(maTK);
            return View(user);
        }

        [HttpPost]
        public ActionResult CapNhatHoSo(string HoTen, string Email, string SDT)
        {
            int maTK = (int)Session["MaTK"];
            var tk = _taiKhoanBLL.LayThongTinAcount(maTK);
            tk.HoTen = HoTen; tk.Email = Email; tk.SDT = SDT;

            if (_taiKhoanBLL.CapNhatHoSo(tk))
            {
                Session["HoTen"] = tk.HoTen;
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            return RedirectToAction("TrangCaNhan");
        }

        [HttpPost]
        public ActionResult DoiMatKhau(string MatKhauCu, string MatKhauMoi)
        {
            int maTK = (int)Session["MaTK"];
            string ketQua = _taiKhoanBLL.DoiMatKhau(maTK, MatKhauCu, MatKhauMoi);
            if (ketQua == "Success") TempData["Success"] = "Đổi mật khẩu thành công!";
            else TempData["Error"] = ketQua;

            return RedirectToAction("TrangCaNhan");
        }

        public ActionResult KhuyenMai()
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");
            int maTK = (int)Session["MaTK"];

            // Lấy danh sách các quán ăn của chủ quán này để đổ vào Dropdown chọn quán
            ViewBag.MaDD = new SelectList(_diaDiemBLL.LayDanhSachTheoChuQuan(maTK), "MaDD", "TenDD");

            var listKM = _khuyenMaiBLL.LayDanhSachKhuyenMai(maTK);
            return View(listKM);
        }

        [HttpPost]
        public ActionResult ThemKhuyenMai(KHUYENMAI km)
        {
            string ketQua = _khuyenMaiBLL.ThemKhuyenMai(km);
            if (ketQua == "Success") TempData["Success"] = "Đã tạo chương trình khuyến mãi mới!";
            else TempData["Error"] = ketQua;

            return RedirectToAction("KhuyenMai");
        }

        [HttpPost]
        public JsonResult XoaKhuyenMai(int maKM)
        {
            bool result = _khuyenMaiBLL.XoaKhuyenMai(maKM);
            return Json(new { success = result });
        }

        //=======================================================================
        public ActionResult PhanHoiDanhGia()
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");

            int maTK = (int)Session["MaTK"];
            var listDanhGia = _phanHoiBLL.LayDanhGiaTheoChuQuan(maTK);

            return View(listDanhGia);
        }

        [HttpPost]
        public ActionResult GuiPhanHoi(int MaDG, string NoiDung)
        {
            if (Session["LoaiTK"] == null || Session["LoaiTK"].ToString() != "ChuCSKD") return RedirectToAction("DangNhap", "TaiKhoan");

            int maTK = (int)Session["MaTK"];
            string ketQua = _phanHoiBLL.GuiPhanHoi(MaDG, NoiDung, maTK);

            if (ketQua == "Success")
            {
                TempData["Success"] = "Đã gửi phản hồi thành công tới khách hàng!";
            }
            else
            {
                TempData["Error"] = ketQua;
            }

            return RedirectToAction("PhanHoiDanhGia");
        }

        [HttpPost]
        public JsonResult ToggleHoatDong(int maDD)
        {
            if (Session["MaTK"] == null) return Json(new { success = false, message = "Chưa đăng nhập" });
            int maTK = Convert.ToInt32(Session["MaTK"]);

            var dd = _diaDiemBLL.LayChiTietDiaDiem(maDD);

            if (dd != null && dd.MaTK == maTK && (dd.TrangThai.Trim() == "DangHoatDong" || dd.TrangThai.Trim() == "TamNgung"))
            {
                string trangThaiMoi = (dd.TrangThai.Trim() == "DangHoatDong") ? "TamNgung" : "DangHoatDong";

                // THAY ĐỔI TẠI ĐÂY: Gọi hàm cập nhật trạng thái nhanh, bypass qua bộ lọc kiểm duyệt
                bool ketQua = _diaDiemBLL.CapNhatTrangThaiNhanh(maDD, trangThaiMoi);

                return Json(new { success = ketQua, trangThaiMoi = trangThaiMoi });
            }
            return Json(new { success = false, message = "Không hợp lệ hoặc quán đang chờ duyệt." });
        }
        [HttpPost]
        public ActionResult HuyCapNhat(int id)
        {
            if (Session["MaTK"] == null) return RedirectToAction("DangNhap", "TaiKhoan");
            int maTK = Convert.ToInt32(Session["MaTK"]);

            if (_diaDiemBLL.HuyYeuCauCapNhat(id, maTK))
            {
                TempData["Success"] = "Đã hủy yêu cầu sửa đổi! Quán của bạn đã trở lại hoạt động bình thường.";
            }
            else
            {
                TempData["Error"] = "Không thể thực hiện yêu cầu này.";
            }
            return RedirectToAction("QuanLyGianHang", "ChuCSKD");
        }
    }
}