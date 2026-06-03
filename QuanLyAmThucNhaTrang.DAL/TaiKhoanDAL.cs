using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyAmThucNhaTrang.DAL
{
    public class TaiKhoanDAL
    {
        // 1. Lấy thông tin tài khoản dựa vào Tên đăng nhập
        public TAIKHOAN LayTaiKhoanTheoTenDN(string tenDangNhap)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Tìm kiếm tài khoản khớp tên đăng nhập (không phân biệt hoa thường ở cấp độ SQL)
                return db.TAIKHOAN.FirstOrDefault(t => t.TenDangNhap == tenDangNhap);
            }
        }

        // 2. Kiểm tra xem Tên ĐN, Email hoặc SĐT đã bị trùng chưa (Dùng cho Đăng ký)
        public bool KiemTraTonTai(string tenDangNhap, string email, string sdt)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.TAIKHOAN.Any(t => t.TenDangNhap == tenDangNhap
                                          || t.Email == email
                                          || t.SDT == sdt);
            }
        }

        // 3. Thêm một tài khoản mới vào CSDL
        public bool ThemTaiKhoan(TAIKHOAN tk)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    db.TAIKHOAN.Add(tk);
                    db.SaveChanges(); // Lệnh này mới thực sự ghi xuống Database
                    return true;
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    string errorMessages = "Lỗi chi tiết: ";
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            // Lấy tên cột bị lỗi và lý do lỗi
                            errorMessages += $"[Cột: {validationError.PropertyName} - Lỗi: {validationError.ErrorMessage}] ";
                        }
                    }
                    // Quăng dòng text chi tiết này ra màn hình
                    throw new System.Exception(errorMessages);
                }
            }
        }

        // ============================================================
        // CÁC HÀM MỚI BỔ SUNG CHO CHỨC NĂNG TRANG CÁ NHÂN & ĐỔI MẬT KHẨU
        // ============================================================

        // 4. Lấy thông tin tài khoản theo Mã tài khoản (ID) để nạp vào trang hồ sơ
        public TAIKHOAN LayTaiKhoanTheoMa(int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.TAIKHOAN.FirstOrDefault(t => t.MaTK == maTK);
            }
        }

        // 5. Cập nhật thông tin hồ sơ tổng thể (Họ tên, Email, Số điện thoại)
        public bool CapNhatThongTin(TAIKHOAN tkCapNhat)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var tk = db.TAIKHOAN.FirstOrDefault(t => t.MaTK == tkCapNhat.MaTK);
                    if (tk != null)
                    {
                        tk.HoTen = tkCapNhat.HoTen;
                        tk.Email = tkCapNhat.Email;
                        tk.SDT = tkCapNhat.SDT;

                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        // 6. Cập nhật riêng mật khẩu mới (đã băm mã hóa SHA-256)
        public bool CapNhatMatKhau(int maTK, string matKhauMoiMaHoa)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var tk = db.TAIKHOAN.FirstOrDefault(t => t.MaTK == maTK);
                    if (tk != null)
                    {
                        tk.MatKhau = matKhauMoiMaHoa;
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }
    }
}