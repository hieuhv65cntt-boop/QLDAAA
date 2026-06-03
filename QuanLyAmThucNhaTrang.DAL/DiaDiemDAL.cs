using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace QuanLyAmThucNhaTrang.DAL
{
    public class DiaDiemDAL
    {
        // Lấy danh sách địa điểm đang hoạt động lên trang chủ
        public List<DIADIEM> LayDanhSachTrangChu()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DIADIEM
                         .Include(d => d.DANHMUC) // Kéo theo tên danh mục (Quán ăn, nhà hàng...)
                         .Include(d => d.HINHANH) // Kéo theo danh sách hình ảnh của quán
                         .Where(d => d.TrangThai == "DangHoatDong")
                         .OrderByDescending(d => d.DiemDanhGiaTB) // Quán nhiều sao xếp lên trước
                         .ToList();
            }
        }
        // Lấy thông tin chi tiết của một địa điểm cụ thể
        public DIADIEM LayChiTietDiaDiem(int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DIADIEM
                         .Include(d => d.DANHMUC) // Lấy thông tin phân loại
                         .Include(d => d.HINHANH) // Lấy danh sách toàn bộ ảnh của quán
                         .Include(d => d.KHUVUC) // Lấy thông tin khu vực để hiện bản đồ
                                                 // Lấy danh sách đánh giá, đồng thời kéo theo thông tin của người đã viết đánh giá đó
                         .Include(d => d.DANHGIA.Select(dg => dg.TAIKHOAN))
                         .FirstOrDefault(d => d.MaDD == maDD);
            }
        }

        // 1. Hàm tìm kiếm và lọc kết hợp (Động)
        public List<DIADIEM> TimKiemVaLoc(string tuKhoa, int? maDM, int? maKV)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Khởi tạo câu truy vấn ban đầu: chỉ lấy những quán Đang hoạt động
                var query = db.DIADIEM
                              .Include(d => d.DANHMUC)
                              .Include(d => d.HINHANH)
                              .Where(d => d.TrangThai == "DangHoatDong");

                // Nếu có nhập từ khóa -> tìm theo Tên quán hoặc Địa chỉ
                if (!string.IsNullOrEmpty(tuKhoa))
                {
                    query = query.Where(d => d.TenDD.Contains(tuKhoa) || d.DiaChiChiTiet.Contains(tuKhoa));
                }

                // Nếu có chọn Danh mục -> lọc theo Danh mục
                if (maDM.HasValue)
                {
                    query = query.Where(d => d.MaDM == maDM.Value);
                }

                // Nếu có chọn Khu vực -> lọc theo Khu vực
                if (maKV.HasValue)
                {
                    query = query.Where(d => d.MaKV == maKV.Value);
                }

                // Sắp xếp quán nhiều sao lên trước và xuất ra danh sách
                return query.OrderByDescending(d => d.DiemDanhGiaTB).ToList();
            }
        }

        // 2. Hàm lấy danh sách Danh mục để làm bộ lọc dropdown/sidebar
        public List<DANHMUC> LayTatCaDanhMuc()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DANHMUC.Where(dm => dm.TrangThai == "HoatDong").ToList();
            }
        }

        // 3. Hàm lấy danh sách Khu vực để làm bộ lọc dropdown/sidebar
        public List<KHUVUC> LayTatCaKhuVuc()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.KHUVUC.ToList();
            }
        }
        // 1. Thêm địa điểm mới và trả về ID vừa tạo
        public int ThemDiaDiemMoi(DIADIEM dd)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                db.DIADIEM.Add(dd);
                db.SaveChanges(); // EF tự động cập nhật ID vào thuộc tính MaDD
                return dd.MaDD;
            }
        }

        // Tầng DAL chỉ cần nhận nguyên Object và lưu vào CSDL
        public bool ThemHinhAnh(HINHANH ha)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    db.HINHANH.Add(ha);
                    db.SaveChanges(); // Lệnh này sẽ chính thức lưu nhiều ảnh vào Database
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool XoaHinhAnh(int maHA)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var anh = db.HINHANH.Find(maHA);
                    if (anh != null)
                    {
                        db.HINHANH.Remove(anh);
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        // NÂNG CẤP: Lấy danh sách địa điểm của chủ quán (Đã khử trùng lặp dòng do Draft Pattern)
        public List<DIADIEM> LayDanhSachTheoChuQuan(int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // 1. Tải toàn bộ các bản ghi thuộc sở hữu của tài khoản này
                var tatCaBanGhi = db.DIADIEM
                                    .Include(d => d.DANHMUC)
                                    .Where(d => d.MaTK == maTK)
                                    .ToList();

                // 2. KỸ THUẬT CỐT LÕI: Nhóm các bản ghi có chung gốc lại với nhau
                // Nếu là bản ghi gốc (MaDD_Goc = null) -> Nhóm theo MaDD
                // Nếu là bản ghi nháp (MaDD_Goc = X)    -> Nhóm theo MaDD_Goc (tức là X)
                var nhomTheoGianHang = tatCaBanGhi.GroupBy(d => d.MaDD_Goc ?? d.MaDD);

                var danhSachHienThi = new List<DIADIEM>();

                foreach (var nhom in nhomTheoGianHang)
                {
                    // Tìm xem trong nhóm này có tồn tại Bản ghi nháp (ChoDuyetSua hoặc TuChoiSua) hay không
                    var banNhapPending = nhom.FirstOrDefault(d => d.MaDD_Goc != null);

                    if (banNhapPending != null)
                    {
                        // Nếu đang có chỉnh sửa, ưu tiên đưa bản nháp vào danh sách 
                        // để giao diện hiển thị đúng trạng thái "Đang chờ duyệt sửa" hoặc "Bị từ chối"
                        danhSachHienThi.Add(banNhapPending);
                    }
                    else
                    {
                        // Nếu không có chỉnh sửa nào, đưa bản ghi gốc duy nhất vào danh sách
                        danhSachHienThi.Add(nhom.First());
                    }
                }

                // Sắp xếp danh sách theo ID giảm dần để đưa các yêu cầu mới thao tác lên trên đầu
                return danhSachHienThi.OrderByDescending(d => d.MaDD).ToList();
            }
        }

        // 2. Cập nhật thông tin địa điểm (Sửa quán ăn)
        public bool CapNhatDiaDiem(DIADIEM ddThayDoi)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var dd = db.DIADIEM.FirstOrDefault(d => d.MaDD == ddThayDoi.MaDD);
                    if (dd != null)
                    {
                        dd.TenDD = ddThayDoi.TenDD;
                        dd.MaDM = ddThayDoi.MaDM;
                        dd.MaKV = ddThayDoi.MaKV;
                        dd.DiaChiChiTiet = ddThayDoi.DiaChiChiTiet;
                        dd.SDT = ddThayDoi.SDT;
                        dd.GioMoCua = ddThayDoi.GioMoCua;
                        dd.GioDongCua = ddThayDoi.GioDongCua;
                        dd.MoTa = ddThayDoi.MoTa;
                        dd.ViDo = ddThayDoi.ViDo;
                        dd.KinhDo = ddThayDoi.KinhDo;
                        dd.TrangThai = ddThayDoi.TrangThai; // Cập nhật lại trạng thái nếu có thay đổi nghiệp vụ

                        dd.LyDoTuChoi = ddThayDoi.LyDoTuChoi;

                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        // Chức năng Hủy / Xóa bản nháp khi Chủ quán không muốn sửa nữa
        public bool HuyYeuCauCapNhat(int maDD, int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    // 1. Tìm bản nháp theo ID và MaTK để đảm bảo an toàn
                    var banNhap = db.DIADIEM.FirstOrDefault(d => d.MaDD == maDD && d.MaTK == maTK);

                    // 2. LOGIC QUAN TRỌNG: 
                    // Nếu bạn không xóa được, hãy thử bỏ bớt điều kiện .Trim() hoặc kiểm tra lại trạng thái trong DB 
                    // xem nó là "TuChoiSua" hay "ChoDuyetSua" (có thể bị thừa khoảng trắng)
                    if (banNhap != null && banNhap.MaDD_Goc.HasValue)
                    {
                        // Xóa ảnh liên quan trước (để tránh lỗi khóa ngoại)
                        var dsAnh = db.HINHANH.Where(a => a.MaDD == maDD).ToList();
                        db.HINHANH.RemoveRange(dsAnh);

                        // Xóa bản nháp
                        db.DIADIEM.Remove(banNhap);
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    // Log lỗi ra để biết tại sao không xóa được
                    System.Diagnostics.Debug.WriteLine("Lỗi khi hủy nháp: " + ex.Message);
                    return false;
                }
            }
        }

        public bool CapNhatTrangThaiNhanh(int maDD, string trangThaiMoi)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var dd = db.DIADIEM.Find(maDD);
                    if (dd != null)
                    {
                        dd.TrangThai = trangThaiMoi; // Cập nhật thẳng trạng thái mới
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        // Hàm xóa yêu cầu phê duyệt dành cho Chủ quán
        public bool XoaYeuCauDangKy(int maDD, int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var dd = db.DIADIEM.FirstOrDefault(d => d.MaDD == maDD && d.MaTK == maTK);

                    // Sử dụng Trim() để phòng hờ lỗi khoảng trắng
                    if (dd != null && (dd.TrangThai.Trim() == "ChoDuyet" || dd.TrangThai.Trim() == "TuChoi"))
                    {
                        // 1. Dọn dẹp bảng HINHANH
                        var dsAnh = db.HINHANH.Where(h => h.MaDD == maDD).ToList();
                        if (dsAnh.Count > 0) db.HINHANH.RemoveRange(dsAnh);

                        // 2. Dọn dẹp bảng KHUYENMAI (Đây chính là chìa khóa fix lỗi!)
                        var dsKhuyenMai = db.KHUYENMAI.Where(k => k.MaDD == maDD).ToList();
                        if (dsKhuyenMai.Count > 0) db.KHUYENMAI.RemoveRange(dsKhuyenMai);

                   
                        db.DIADIEM.Remove(dd);
                        db.SaveChanges();

                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
