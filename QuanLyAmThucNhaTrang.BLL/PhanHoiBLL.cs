using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class PhanHoiBLL
    {
        private PhanHoiDAL _phanHoiDAL = new PhanHoiDAL();

        public List<DANHGIA> LayDanhGiaTheoChuQuan(int maTK)
        {
            return _phanHoiDAL.LayDanhGiaTheoChuQuan(maTK);
        }

        public string GuiPhanHoi(int maDG, string noiDung, int maTKChuQuan)
        {
            PHANHOI ph = new PHANHOI
            {
                MaDG = maDG,
                NoiDung = noiDung,
                MaTK = maTKChuQuan,
                NgayPhanHoi = DateTime.Now
            };

            if (_phanHoiDAL.ThemPhanHoi(ph))
            {
                return "Success";
            }
            return "Có lỗi xảy ra khi gửi phản hồi. Vui lòng thử lại.";
        }
    }
}
