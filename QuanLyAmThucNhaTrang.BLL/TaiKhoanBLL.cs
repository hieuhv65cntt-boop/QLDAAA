using QuanLyAmThucNhaTrang.DAL; // Gọi reference xuống DAL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class TaiKhoanBLL
    {
        private TaiKhoanDAL _taiKhoanDAL = new TaiKhoanDAL();

        // Hàm tiện ích: Băm mật khẩu (Mã hóa 1 chiều) bằng SHA-256
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return "";

            using (SHA256 sha256 = SHA256.Create())
            {
                // Chuyển chuỗi mật khẩu thành mảng byte
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển mảng byte về chuỗi Hexadecimal
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // XỬ LÝ NGHIỆP VỤ: ĐĂNG KÝ
        public string DangKy(TAIKHOAN tkMoi)
        {
            // 1. Kiểm tra dữ liệu trùng lặp
            if (_taiKhoanDAL.KiemTraTonTai(tkMoi.TenDangNhap, tkMoi.Email, tkMoi.SDT))
            {
                return "Tên đăng nhập, Email hoặc Số điện thoại này đã được sử dụng!";
            }

            // 2. Mã hóa mật khẩu trước khi lưu
            tkMoi.MatKhau = HashPassword(tkMoi.MatKhau);

            // 3. Khởi tạo các giá trị mặc định an toàn
            tkMoi.NgayTao = DateTime.Now;
            // Giả sử: 1 = Đang hoạt động, 0 = Bị khóa
            tkMoi.TrangThai = "HoatDong"; 

            // 4. Lưu xuống DB qua DAL
            if (_taiKhoanDAL.ThemTaiKhoan(tkMoi))
            {
                return "Success";
            }
            return "Có lỗi hệ thống trong quá trình tạo tài khoản. Vui lòng thử lại sau.";
        }

        // XỬ LÝ NGHIỆP VỤ: ĐĂNG NHẬP
        public TAIKHOAN DangNhap(string tenDangNhap, string matKhauGoc)
        {
            // Lấy tài khoản lên từ DAL
            TAIKHOAN tk = _taiKhoanDAL.LayTaiKhoanTheoTenDN(tenDangNhap);

            if (tk != null)
            {
                // Mã hóa mật khẩu người dùng vừa nhập...
                string matKhauMaHoa = HashPassword(matKhauGoc);

                // ... và so sánh với mật khẩu đã mã hóa lưu trong Database
                if (tk.MatKhau == matKhauMaHoa)
                {
                    // (Tùy chọn) Kiểm tra nếu tk.TrangThai == 0 thì có thể return null (Không cho đăng nhập)
                    return tk; // Đăng nhập thành công, trả về toàn bộ thông tin user
                }
            }

            // Trả về null nếu sai tên đăng nhập hoặc sai mật khẩu
            return null;
        }

        public TAIKHOAN LayThongTinAcount(int maTK)
        {
            return _taiKhoanDAL.LayTaiKhoanTheoMa(maTK);
        }

        public bool CapNhatHoSo(TAIKHOAN tk)
        {
            // Có thể bổ sung kiểm tra định dạng Email/SĐT ở đây nếu cần
            return _taiKhoanDAL.CapNhatThongTin(tk);
        }

        // Nghiệp vụ đổi mật khẩu an toàn
        public string DoiMatKhau(int maTK, string matKhauCu, string matKhauMoi)
        {
            var tk = _taiKhoanDAL.LayTaiKhoanTheoMa(maTK);
            if (tk == null) return "Tài khoản không tồn tại.";

            // 1. Mã hóa mật khẩu cũ người dùng nhập vào để đối chiếu với database
            string matKhauCuMaHoa = HashPassword(matKhauCu);
            if (tk.MatKhau != matKhauCuMaHoa)
            {
                return "Mật khẩu cũ không chính xác!";
            }

            // 2. Mã hóa mật khẩu mới và ra lệnh cập nhật
            string matKhauMoiMaHoa = HashPassword(matKhauMoi);
            if (_taiKhoanDAL.CapNhatMatKhau(maTK, matKhauMoiMaHoa))
            {
                return "Success";
            }
            return "Có lỗi hệ thống xảy ra khi đổi mật khẩu.";
        }
    }
}
