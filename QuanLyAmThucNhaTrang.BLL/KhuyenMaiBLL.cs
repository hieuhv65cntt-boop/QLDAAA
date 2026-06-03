using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class KhuyenMaiBLL
    {
        private KhuyenMaiDAL _kmDAL = new KhuyenMaiDAL();

        public List<KHUYENMAI> LayDanhSachKhuyenMai(int maTK)
        {
            return _kmDAL.LayDanhSachKhuyenMai(maTK);
        }

        public string ThemKhuyenMai(KHUYENMAI km)
        {
            // Kiểm tra quy định nghiệp vụ (QĐ7)
            if (km.NgayBatDau > km.NgayKetThuc)
                return "Lỗi: Ngày bắt đầu không được lớn hơn ngày kết thúc!";

            km.TrangThai = "ConHieuLuc";

            if (_kmDAL.ThemKhuyenMai(km)) return "Success";
            return "Có lỗi xảy ra khi thêm dữ liệu.";
        }

        public bool XoaKhuyenMai(int maKM)
        {
            return _kmDAL.XoaKhuyenMai(maKM);
        }

        public List<KHUYENMAI> LayKhuyenMaiHieuLuc(int maDD)
        {
            return _kmDAL.LayKhuyenMaiHieuLuc(maDD);
        }
    }
}
