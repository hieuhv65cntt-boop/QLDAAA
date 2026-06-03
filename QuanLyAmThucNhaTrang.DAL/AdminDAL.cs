using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace QuanLyAmThucNhaTrang.DAL
{
    public class AdminDAL
    {
        // 1. Lấy danh sách các địa điểm đang chờ duyệt
        public List<DIADIEM> LayDanhSachChoDuyet()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Kéo theo bảng TAIKHOAN (Chủ quán) và DANHMUC để hiển thị thông tin
                return db.DIADIEM.Include(d => d.TAIKHOAN)
                                 .Include(d => d.DANHMUC)
                                 .Include(d => d.HINHANH)
                                 .Where(d => d.TrangThai == "ChoDuyet" || d.TrangThai == "ChoDuyetSua")
                                 .OrderBy(d => d.NgayDangKy) // Ưu tiên người đăng ký trước
                                 .ToList();
            }
        }

        // 2. Phê duyệt địa điểm (Đã nâng cấp chuẩn kiến trúc Bản sao dữ liệu - Draft Pattern)
        public bool PheDuyetDiaDiem(int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Dùng Transaction để đảm bảo: Nếu copy lỗi thì không xóa nháp
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Tìm bản nháp
                        var banNhap = db.DIADIEM.Include("HINHANH").FirstOrDefault(d => d.MaDD == maDD);
                        if (banNhap == null) return false;

                        // 2. Nếu là duyệt SỬA (có MaDD_Goc)
                        if (banNhap.MaDD_Goc.HasValue)
                        {
                            var quanGoc = db.DIADIEM.Find(banNhap.MaDD_Goc.Value);
                            if (quanGoc != null)
                            {
                                // Cập nhật thông tin từ bản nháp sang bản gốc
                                quanGoc.TenDD = banNhap.TenDD;
                                quanGoc.DiaChiChiTiet = banNhap.DiaChiChiTiet;
                                quanGoc.SDT = banNhap.SDT;
                                quanGoc.GioMoCua = banNhap.GioMoCua;
                                quanGoc.GioDongCua = banNhap.GioDongCua;
                                quanGoc.MaDM = banNhap.MaDM;
                                quanGoc.MaKV = banNhap.MaKV;
                                quanGoc.ViDo = banNhap.ViDo;
                                quanGoc.KinhDo = banNhap.KinhDo;
                                quanGoc.MoTa = banNhap.MoTa;
                                quanGoc.TrangThai = "DangHoatDong";

                                // Xử lý ảnh: Xóa ảnh cũ của bản gốc, copy ảnh từ bản nháp sang
                                var anhCu = db.HINHANH.Where(a => a.MaDD == quanGoc.MaDD).ToList();
                                db.HINHANH.RemoveRange(anhCu);

                                foreach (var anhNhap in banNhap.HINHANH)
                                {
                                    db.HINHANH.Add(new HINHANH
                                    {
                                        MaDD = quanGoc.MaDD,
                                        DuongDan = anhNhap.DuongDan,
                                        LoaiHinhAnh = anhNhap.LoaiHinhAnh
                                    });
                                }
                            }
                            // Xóa bản nháp sau khi đã cập nhật xong bản gốc
                            db.HINHANH.RemoveRange(banNhap.HINHANH);
                            db.DIADIEM.Remove(banNhap);
                        }
                        else
                        {
                            // Nếu là duyệt đăng ký mới (không có MaDD_Goc)
                            banNhap.TrangThai = "DangHoatDong";
                        }

                        db.SaveChanges(); // Lưu tất cả thay đổi cùng lúc
                        transaction.Commit(); // Chốt giao dịch
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Quay lại trạng thái cũ nếu có lỗi
                        System.Diagnostics.Debug.WriteLine("Lỗi phê duyệt: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        public bool TuChoiDiaDiem(int maDD, string lyDo)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var dd = db.DIADIEM.Find(maDD);
                    if (dd != null)
                    {
                        // BỘ NÃO PHÂN TÍCH NẰM Ở ĐÂY:
                        if (dd.TrangThai.Trim() == "ChoDuyet")
                        {
                            dd.TrangThai = "TuChoi"; // Đơn mới -> Từ chối hẳn
                        }
                        else if (dd.TrangThai.Trim() == "ChoDuyetSua")
                        {
                            dd.TrangThai = "TuChoiSua"; // Đơn cũ xin sửa -> Trả về để sửa lại, vẫn cho bán
                        }

                        dd.LyDoTuChoi = lyDo; // Gắn lý do "Ảnh quá mờ" vào
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        public System.Collections.Generic.Dictionary<string, int> ThongKeNhanh()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return new System.Collections.Generic.Dictionary<string, int>
                {
                    // Đếm số lượng quán ăn đang chờ duyệt
                    { "ChoDuyet", db.DIADIEM.Count(d => d.TrangThai == "ChoDuyet") },
                    
                    // Đếm số lượng quán ăn đang hoạt động
                    { "HoatDong", db.DIADIEM.Count(d => d.TrangThai == "DangHoatDong") },
                    
                    // Đếm tổng số tài khoản hệ thống (Khách + Chủ quán)
                    { "TaiKhoan", db.TAIKHOAN.Count() },
                    
                    // Đếm tổng số lượt đánh giá
                    { "DanhGia", db.DANHGIA.Count() }
                };
            }
        }

        // --------------------------------------------------------
        // QUẢN LÝ ĐÁNH GIÁ & BÌNH LUẬN
        // --------------------------------------------------------

        // 1. Lấy danh sách đánh giá CÓ BỘ LỌC
        public List<DANHGIA> LayDanhSachDanhGia(int? soSao = null, string tuKhoa = "")
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                var query = db.DANHGIA.Include(d => d.TAIKHOAN)
                                      .Include(d => d.DIADIEM)
                                      .AsQueryable();

                // Lọc theo số sao nếu Admin có chọn
                if (soSao.HasValue && soSao.Value > 0)
                {
                    query = query.Where(d => d.SoSao == soSao.Value);
                }

                // Lọc theo từ khóa (Tìm trong Nội dung bình luận hoặc Tên quán)
                if (!string.IsNullOrEmpty(tuKhoa))
                {
                    tuKhoa = tuKhoa.ToLower();
                    query = query.Where(d => d.NoiDung.ToLower().Contains(tuKhoa) ||
                                             d.DIADIEM.TenDD.ToLower().Contains(tuKhoa));
                }

                // Sắp xếp: Ưu tiên ngày mới nhất
                return query.OrderByDescending(d => d.NgayDanhGia).ToList();
            }
        }

        // 2. Ẩn đánh giá (Chuyển trạng thái)
        public bool AnDanhGia(int maDG)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                var dg = db.DANHGIA.Find(maDG);
                if (dg != null)
                {
                    dg.TrangThai = "DaAn"; // Đổi trạng thái theo quy định
                    db.SaveChanges();
                    CapNhatDiemTrungBinh(dg.MaDD, db); // Gọi hàm tính lại điểm[cite: 1]
                    return true;
                }
                return false;
            }
        }

        // 3. Xóa vĩnh viễn đánh giá (Xóa luôn cả Phản hồi nếu có)
        public bool XoaDanhGia(int maDG)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var dg = db.DANHGIA.Find(maDG);
                        if (dg != null)
                        {
                            int maDD = dg.MaDD;

                            // Bước a: Xóa Phản hồi của chủ quán trước (ràng buộc khóa ngoại)[cite: 1]
                            var ph = db.PHANHOI.FirstOrDefault(p => p.MaDG == maDG);
                            if (ph != null) db.PHANHOI.Remove(ph);

                            // Bước b: Xóa Đánh giá[cite: 1]
                            db.DANHGIA.Remove(dg);
                            db.SaveChanges();

                            // Bước c: Tính lại điểm trung bình cho quán ăn[cite: 1]
                            CapNhatDiemTrungBinh(maDD, db);

                            transaction.Commit();
                            return true;
                        }
                        return false;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        // 4. Hàm phụ trợ: Tính lại điểm trung bình (Công thức CT1)[cite: 1]
        private void CapNhatDiemTrungBinh(int maDD, QuanLyAmThucNhaTrangEntities db)
        {
            // Lấy các đánh giá hợp lệ (HienThi) của quán
            var dsDanhGiaHienThi = db.DANHGIA.Where(x => x.MaDD == maDD && x.TrangThai == "HienThi").ToList();
            var dd = db.DIADIEM.Find(maDD);

            if (dd != null)
            {
                dd.SoLuotDanhGia = dsDanhGiaHienThi.Count;
                if (dsDanhGiaHienThi.Count > 0)
                {
                    // Tính trung bình số sao
                    dd.DiemDanhGiaTB = Math.Round(dsDanhGiaHienThi.Average(x => x.SoSao), 1);
                }
                else
                {
                    dd.DiemDanhGiaTB = 0;
                }
                db.SaveChanges();
            }
        }

        // 3. QUẢN LÝ DANH MỤC ẨM THỰC
        // ========================================================
        public List<DANHMUC> LayDanhSachDanhMuc()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DANHMUC.ToList();
            }
        }

        public bool KiemTraTrungTenDM(string tenDM, int maDMIgnore = 0)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DANHMUC.Any(d => d.TenDM.ToLower() == tenDM.ToLower() && d.MaDM != maDMIgnore);
            }
        }

        public bool ThemDanhMuc(DANHMUC dm)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try { db.DANHMUC.Add(dm); db.SaveChanges(); return true; }
                catch { return false; }
            }
        }

        public bool CapNhatDanhMuc(DANHMUC dmCapNhat)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var dm = db.DANHMUC.Find(dmCapNhat.MaDM);
                    if (dm != null) { dm.TenDM = dmCapNhat.TenDM; dm.MoTa = dmCapNhat.MoTa; db.SaveChanges(); return true; }
                    return false;
                }
                catch { return false; }
            }
        }

        // Xóa Danh Mục theo quy định QĐ11 trong báo cáo
        public bool XoaDanhMuc(int maDM)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var dm = db.DANHMUC.Find(maDM);
                    if (dm != null)
                    {
                        // Kiểm tra xem có địa điểm nào đang dùng danh mục này không
                        bool coDiaDiem = db.DIADIEM.Any(d => d.MaDM == maDM);
                        if (coDiaDiem)
                        {
                            // Nếu có, chỉ chuyển trạng thái (Không xóa cứng để bảo toàn dữ liệu cũ)
                            dm.TrangThai = "NgungSuDung";
                        }
                        else
                        {
                            // Nếu chưa có quán nào dùng, xóa vĩnh viễn
                            db.DANHMUC.Remove(dm);
                        }
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }


        public List<BaoCaoDiaDiemVM> ThongKeDiaDiem(System.DateTime tuNgay, System.DateTime denNgay)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                var result = new List<BaoCaoDiaDiemVM>();
                var dsDanhMuc = db.DANHMUC.ToList();

                foreach (var dm in dsDanhMuc)
                {
                    var query = db.DIADIEM.Where(d => d.MaDM == dm.MaDM && d.NgayDangKy >= tuNgay && d.NgayDangKy <= denNgay);

                    int hoatDong = query.Count(d => d.TrangThai == "DangHoatDong");
                    int tamNgung = query.Count(d => d.TrangThai == "ChoDuyet" || d.TrangThai == "TamNgung");
                    int daDong = query.Count(d => d.TrangThai == "TuChoi");

                    result.Add(new BaoCaoDiaDiemVM
                    {
                        LoaiHinhAmThuc = dm.TenDM,
                        DangHoatDong = hoatDong,
                        TamNgung = tamNgung,
                        DaDongCua = daDong,
                        TongCong = hoatDong + tamNgung + daDong
                    });
                }
                return result;
            }
        }

        // 6. BÁO CÁO THỐNG KÊ TÀI KHOẢN (BM_07)
        // ========================================================
        public List<BaoCaoTaiKhoanVM> ThongKeTaiKhoan(System.DateTime tuNgay, System.DateTime denNgay)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                var result = new List<BaoCaoTaiKhoanVM>();

                // Thống kê nhóm Khách Hàng
                int tongKH = db.TAIKHOAN.Count(t => t.LoaiTK == "KhachHang");
                int moiKH = db.TAIKHOAN.Count(t => t.LoaiTK == "KhachHang" && t.NgayTao >= tuNgay && t.NgayTao <= denNgay);

                // Thống kê nhóm Chủ Cơ Sở Kinh Doanh
                int tongChu = db.TAIKHOAN.Count(t => t.LoaiTK == "ChuCSKD");
                int moiChu = db.TAIKHOAN.Count(t => t.LoaiTK == "ChuCSKD" && t.NgayTao >= tuNgay && t.NgayTao <= denNgay);

                int tongTaiKhoan = tongKH + tongChu; // Tổng dùng để chia tỷ lệ

                // Nạp dữ liệu vào danh sách kết quả
                result.Add(new BaoCaoTaiKhoanVM
                {
                    NhomDoiTuong = "Khách hàng",
                    DangKyMoi = moiKH,
                    TongSo = tongKH,
                    TyLe = tongTaiKhoan > 0 ? System.Math.Round((double)tongKH / tongTaiKhoan * 100, 2) : 0
                });

                result.Add(new BaoCaoTaiKhoanVM
                {
                    NhomDoiTuong = "Chủ cơ sở kinh doanh",
                    DangKyMoi = moiChu,
                    TongSo = tongChu,
                    TyLe = tongTaiKhoan > 0 ? System.Math.Round((double)tongChu / tongTaiKhoan * 100, 2) : 0
                });

                return result;
            }
        }

        // 7. BÁO CÁO THỐNG KÊ ĐÁNH GIÁ (BM_08)
        // ========================================================
        public List<BaoCaoDanhGiaVM> ThongKeDanhGia(System.DateTime tuNgay, System.DateTime denNgay)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                var result = new List<BaoCaoDanhGiaVM>();

                // Lấy các địa điểm đang hoạt động để làm báo cáo
                var danhSachDD = db.DIADIEM.Where(d => d.TrangThai == "DangHoatDong").ToList();

                foreach (var dd in danhSachDD)
                {
                    // Lọc ra các đánh giá thuộc về địa điểm này VÀ nằm trong khoảng thời gian báo cáo
                    var dsDanhGiaTrongKy = db.DANHGIA.Where(dg => dg.MaDD == dd.MaDD
                                                               && dg.NgayDanhGia >= tuNgay
                                                               && dg.NgayDanhGia <= denNgay).ToList();

                    // Chỉ đưa vào báo cáo những quán có phát sinh đánh giá trong kỳ này
                    if (dsDanhGiaTrongKy.Count > 0)
                    {
                        int soLuot = dsDanhGiaTrongKy.Count;
                        // Đếm số đánh giá đã bị Admin Ẩn hoặc Xóa
                        int soViPham = dsDanhGiaTrongKy.Count(dg => dg.TrangThai == "DaAn" || dg.TrangThai == "DaXoa");

                        result.Add(new BaoCaoDanhGiaVM
                        {
                            TenDiaDiem = dd.TenDD,
                            SoLuotDanhGia = soLuot,
                            DiemDanhGiaTB = dd.DiemDanhGiaTB, // Điểm trung bình tổng thể của quán
                            SoViPham = soViPham
                        });
                    }
                }

                // Sắp xếp các quán có lượt đánh giá cao nhất (hot nhất) lên đầu
                return result.OrderByDescending(x => x.SoLuotDanhGia).ToList();
            }
        }
    }
}
