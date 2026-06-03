using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class AdminBLL
    {
        private AdminDAL _adminDAL = new AdminDAL();

        public List<DIADIEM> LayDanhSachChoDuyet()
        {
            return _adminDAL.LayDanhSachChoDuyet();
        }

        // Nghiệp vụ Phê Duyệt (Đã chuẩn hóa theo Draft Pattern)
        public bool PheDuyetDiaDiem(int maDD)
        {
            // Chỉ cần truyền đúng Mã địa điểm, DAL sẽ tự động lo việc Merge dữ liệu và cập nhật trạng thái
            return _adminDAL.PheDuyetDiaDiem(maDD);
        }

        public bool TuChoiDiaDiem(int maDD, string lyDo)
        {
            return _adminDAL.TuChoiDiaDiem(maDD, lyDo);
        }

        public System.Collections.Generic.Dictionary<string, int> ThongKeNhanh()
        {
            return _adminDAL.ThongKeNhanh();
        }

        public List<DANHGIA> LayDanhSachDanhGia(int? soSao = null, string tuKhoa = "")
        {
            return _adminDAL.LayDanhSachDanhGia(soSao, tuKhoa);
        }

        public bool AnDanhGia(int maDG)
        {
            return _adminDAL.AnDanhGia(maDG);
        }

        public bool XoaDanhGia(int maDG)
        {
            return _adminDAL.XoaDanhGia(maDG);
        }

        // --- NGHIỆP VỤ DANH MỤC ---
        public List<DANHMUC> LayDanhSachDanhMuc() => _adminDAL.LayDanhSachDanhMuc();

        public string ThemDanhMuc(DANHMUC dm)
        {
            if (string.IsNullOrWhiteSpace(dm.TenDM)) return "Tên danh mục không được để trống!";
            if (_adminDAL.KiemTraTrungTenDM(dm.TenDM)) return "Tên danh mục này đã tồn tại!";

            dm.TrangThai = "HoatDong";
            if (_adminDAL.ThemDanhMuc(dm)) return "Success";
            return "Lỗi hệ thống khi thêm danh mục.";
        }

        public string CapNhatDanhMuc(DANHMUC dm)
        {
            if (string.IsNullOrWhiteSpace(dm.TenDM)) return "Tên danh mục không được để trống!";
            if (_adminDAL.KiemTraTrungTenDM(dm.TenDM, dm.MaDM)) return "Tên danh mục này bị trùng với danh mục khác!";

            if (_adminDAL.CapNhatDanhMuc(dm)) return "Success";
            return "Lỗi hệ thống khi cập nhật.";
        }

        public List<BaoCaoDiaDiemVM> ThongKeDiaDiem(System.DateTime tuNgay, System.DateTime denNgay)
        {
            return _adminDAL.ThongKeDiaDiem(tuNgay, denNgay);
        }

        public List<BaoCaoTaiKhoanVM> ThongKeTaiKhoan(System.DateTime tuNgay, System.DateTime denNgay)
        {
            return _adminDAL.ThongKeTaiKhoan(tuNgay, denNgay);
        }

        public List<BaoCaoDanhGiaVM> ThongKeDanhGia(System.DateTime tuNgay, System.DateTime denNgay)
        {
            return _adminDAL.ThongKeDanhGia(tuNgay, denNgay);
        }

        public bool XoaDanhMuc(int maDM) => _adminDAL.XoaDanhMuc(maDM);

        


    }
}
