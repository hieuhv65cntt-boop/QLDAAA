using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class DanhGiaBLL
    {
        private DanhGiaDAL _danhGiaDAL = new DanhGiaDAL();

        public string GuiDanhGia(int maTK, int maDD, int soSao, string noiDung)
        {
            // 1. Kiểm tra QĐ3: Đã đánh giá chưa?
            if (_danhGiaDAL.KiemTraDaDanhGia(maTK, maDD))
            {
                return "Bạn đã gửi đánh giá cho địa điểm này rồi. Không thể đánh giá thêm!";
            }

            // 2. Tạo đối tượng đánh giá mới
            DANHGIA dgMoi = new DANHGIA
            {
                MaTK = maTK,
                MaDD = maDD,
                SoSao = soSao,
                NoiDung = noiDung,
                NgayDanhGia = DateTime.Now,
                TrangThai = "HienThi"
            };

            // 3. Lưu xuống Database
            if (_danhGiaDAL.ThemDanhGia(dgMoi))
            {
                // 4. Nếu lưu thành công, ra lệnh cập nhật lại điểm trung bình cho quán
                _danhGiaDAL.CapNhatDiemTrungBinh(maDD);
                return "Success";
            }

            return "Có lỗi xảy ra khi lưu đánh giá. Vui lòng thử lại.";
        }

        public List<DANHGIA> LayLichSuDanhGiaTheoUser(int maTK)
        {
            return _danhGiaDAL.LayLichSuDanhGiaTheoUser(maTK);
        }
    }
}
