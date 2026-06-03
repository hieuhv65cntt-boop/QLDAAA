using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class YeuThichBLL
    {
        private YeuThichDAL _yeuThichDAL = new YeuThichDAL();

        // Hàm xử lý logic Toggle (Bật/Tắt) thông minh
        public string XulyYeuThich(int maTK, int maDD)
        {
            // Nếu đã lưu rồi -> Ra lệnh Xóa
            if (_yeuThichDAL.KiemTraDaLuu(maTK, maDD))
            {
                bool ketQua = _yeuThichDAL.XoaYeuThich(maTK, maDD);
                return ketQua ? "DaXoa" : "Loi";
            }
            // Nếu chưa lưu -> Ra lệnh Thêm
            else
            {
                bool ketQua = _yeuThichDAL.ThemYeuThich(maTK, maDD);
                return ketQua ? "DaLuu" : "Loi";
            }
        }

        // Chuyển tiếp danh sách cho Controller gọi
        public List<YEUTHICH> LayDanhSachYeuThich(int maTK)
        {
            return _yeuThichDAL.LayDanhSachYeuThich(maTK);
        }

        // Dùng để kiểm tra trạng thái lúc load trang Chi tiết (để tô màu nút đỏ hay trắng)
        public bool KiemTraTrangThaiLuu(int maTK, int maDD)
        {
            return _yeuThichDAL.KiemTraDaLuu(maTK, maDD);
        }
    }
}
