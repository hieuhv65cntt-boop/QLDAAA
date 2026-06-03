using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class DiaDiemBLL
    {
        private DiaDiemDAL _diaDiemDAL = new DiaDiemDAL();

        public List<DIADIEM> TimKiemVaLoc(string tuKhoa, int? maDM, int? maKV)
        {
            return _diaDiemDAL.TimKiemVaLoc(tuKhoa, maDM, maKV);
        }

        public List<DANHMUC> LayTatCaDanhMuc()
        {
            // NGHIỆP VỤ MỚI BỔ SUNG (QĐ11): 
            // Dùng LINQ để lọc, chỉ trả về các Danh mục đang ở trạng thái "HoatDong".
            // Tránh trường hợp Chủ quán chọn nhầm vào Danh mục đã bị Admin "NgungSuDung".
            return _diaDiemDAL.LayTatCaDanhMuc()
                              .Where(dm => dm.TrangThai == "HoatDong")
                              .ToList();
        }

        public List<KHUVUC> LayTatCaKhuVuc()
        {
            return _diaDiemDAL.LayTatCaKhuVuc();
        }

        public List<DIADIEM> LayDanhSachTrangChu()
        {
            return _diaDiemDAL.LayDanhSachTrangChu();
        }

        public DIADIEM LayChiTietDiaDiem(int maDD)
        {
            return _diaDiemDAL.LayChiTietDiaDiem(maDD);
        }

        public bool XoaYeuCauDangKy(int maDD, int maTK)
        {
            return _diaDiemDAL.XoaYeuCauDangKy(maDD, maTK);
        }

        // Thêm địa điểm với các giá trị mặc định ban đầu (Đăng ký mới tinh)
        public int ThemDiaDiemMoi(DIADIEM dd)
        {
            dd.TrangThai = "ChoDuyet"; // Bắt buộc chờ Admin duyệt cấp phép
            dd.DiemDanhGiaTB = 0;
            dd.SoLuotDanhGia = 0;
            dd.NgayDangKy = DateTime.Now;
            dd.MaDD_Goc = null; // Đơn mới tinh thì không có bản ghi gốc

            return _diaDiemDAL.ThemDiaDiemMoi(dd);
        }

        // Thêm hình ảnh
        public bool ThemHinhAnh(int maDD, string duongDan, string loaiHinhAnh)
        {
            HINHANH ha = new HINHANH
            {
                MaDD = maDD,
                DuongDan = duongDan,
                LoaiHinhAnh = loaiHinhAnh
            };
            return _diaDiemDAL.ThemHinhAnh(ha);
        }

        public bool XoaHinhAnh(int maHA)
        {
            return _diaDiemDAL.XoaHinhAnh(maHA);
        }

        public List<DIADIEM> LayDanhSachTheoChuQuan(int maTK)
        {
            return _diaDiemDAL.LayDanhSachTheoChuQuan(maTK);
        }

        /// <summary>
        /// HÀM NÂNG CẤP: Xử lý chỉnh sửa thông tin gian hàng theo cơ chế Bản sao dữ liệu (Draft Pattern)
        /// </summary>
        public int CapNhatGianHang(DIADIEM dd)
        {
            // 1. Kiểm tra trạng thái hiện tại của thực thể trong Cơ sở dữ liệu
            var quanHienTai = _diaDiemDAL.LayChiTietDiaDiem(dd.MaDD);

            // LỖI 1 ĐÃ SỬA: Đổi return false thành return -1
            if (quanHienTai == null) return -1;

            // THỨ NHẤT: Nếu thực thể đang sửa đổi là BẢN GHI GỐC (Chưa từng nhân bản)
            if (quanHienTai.MaDD_Goc == null)
            {
                string trangThaiGoc = quanHienTai.TrangThai.Trim();

                // Nếu quán gốc Đang hoạt động hoặc Tạm ngưng -> TIẾN HÀNH NHÂN BẢN DÒNG MỚI (INSERT)
                if (trangThaiGoc == "DangHoatDong" || trangThaiGoc == "TamNgung")
                {
                    DIADIEM banNhapMoi = new DIADIEM
                    {
                        TenDD = dd.TenDD,
                        DiaChiChiTiet = dd.DiaChiChiTiet,
                        SDT = dd.SDT,
                        GioMoCua = dd.GioMoCua,
                        GioDongCua = dd.GioDongCua,
                        MaDM = dd.MaDM,
                        MaKV = dd.MaKV,
                        ViDo = dd.ViDo,
                        KinhDo = dd.KinhDo,
                        MoTa = dd.MoTa,
                        MaTK = quanHienTai.MaTK,
                        NgayDangKy = DateTime.Now,

                        TrangThai = "ChoDuyetSua",
                        MaDD_Goc = quanHienTai.MaDD
                    };

                    // LỖI 2 ĐÃ SỬA: Hàm ThemDiaDiemMoi vốn đã trả về INT (Mã bản nháp), 
                    // nên ta return trực tiếp nó luôn, bỏ cái "> 0" đi
                    return _diaDiemDAL.ThemDiaDiemMoi(banNhapMoi);
                }
                // Nếu đơn gốc là đơn đăng ký mới hoàn toàn đang bị từ chối (TuChoi) -> CẬP NHẬT TRỰC TIẾP (UPDATE)
                else
                {
                    if (trangThaiGoc == "TuChoi") dd.TrangThai = "ChoDuyet";
                    dd.LyDoTuChoi = null;

                    // LỖI 3 ĐÃ SỬA: Nếu update thành công thì trả về ID quán, thất bại thì trả về -1
                    return _diaDiemDAL.CapNhatDiaDiem(dd) ? dd.MaDD : -1;
                }
            }

            // THỨ HAI: Nếu thực thể đang sửa đổi ĐÃ LÀ BẢN GHI NHÁP (Đã có MaDD_Goc)
            // Tức là chủ quán đang sửa lại lỗi trên bản nháp vừa bị Admin từ chối (TuChoiSua)
            else
            {
                dd.TrangThai = "ChoDuyetSua"; // Đưa trạng thái bản nháp quay lại Chờ duyệt sửa
                dd.MaDD_Goc = quanHienTai.MaDD_Goc; // Giữ nguyên liên kết tới quán gốc
                dd.LyDoTuChoi = null; // Gỡ bỏ dòng lý do từ chối cũ để Admin chấm lại từ đầu

                // LỖI 4 ĐÃ SỬA: Nếu update thành công thì trả về ID bản nháp, thất bại trả về -1
                return _diaDiemDAL.CapNhatDiaDiem(dd) ? dd.MaDD : -1;
            }
        }

        // Chức năng Chủ quán chủ động hủy/xóa bỏ vĩnh viễn bản sao nháp khi không muốn sửa thông tin nữa
        public bool HuyYeuCauCapNhat(int maDD, int maTK)
        {
            // Forward chuyển tiếp tham số an toàn xuống tầng DAL xử lý lệnh DELETE bản nháp
            return _diaDiemDAL.HuyYeuCauCapNhat(maDD, maTK);
        }

        public bool CapNhatTrangThaiNhanh(int maDD, string trangThaiMoi)
        {
            return _diaDiemDAL.CapNhatTrangThaiNhanh(maDD, trangThaiMoi);
        }
    }
}