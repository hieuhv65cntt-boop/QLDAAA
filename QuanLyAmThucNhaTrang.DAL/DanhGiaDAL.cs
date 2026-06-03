using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace QuanLyAmThucNhaTrang.DAL
{
    public class DanhGiaDAL
    {
        // 1. Kiểm tra xem user này đã đánh giá quán này chưa (Quy định 3)
        public bool KiemTraDaDanhGia(int maTK, int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DANHGIA.Any(dg => dg.MaTK == maTK && dg.MaDD == maDD);
            }
        }

        // 2. Thêm đánh giá mới vào bảng DANHGIA
        public bool ThemDanhGia(DANHGIA dg)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    db.DANHGIA.Add(dg);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        // 3. Tự động tính toán lại Điểm Trung Bình & Số Lượt cho bảng DIADIEM
        public void CapNhatDiemTrungBinh(int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Lấy tất cả đánh giá hợp lệ của quán này
                var danhSachDG = db.DANHGIA.Where(dg => dg.MaDD == maDD && dg.TrangThai == "HienThi").ToList();
                var diaDiem = db.DIADIEM.FirstOrDefault(d => d.MaDD == maDD);

                if (diaDiem != null)
                {
                    diaDiem.SoLuotDanhGia = danhSachDG.Count;
                    // Nếu có đánh giá thì tính trung bình (làm tròn 1 chữ số), nếu không thì cho bằng 0
                    diaDiem.DiemDanhGiaTB = danhSachDG.Count > 0 ? Math.Round(danhSachDG.Average(d => d.SoSao), 1) : 0;

                    db.SaveChanges();
                }
            }
        }

        public List<DANHGIA> LayLichSuDanhGiaTheoUser(int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.DANHGIA
                         .Include(dg => dg.DIADIEM) // Kéo theo thông tin địa điểm được đánh giá
                         .Include(dg => dg.DIADIEM.DANHMUC) // Kéo theo danh mục của địa điểm đó
                         .Where(dg => dg.MaTK == maTK && dg.TrangThai == "HienThi")
                         .OrderByDescending(dg => dg.NgayDanhGia) // Đánh giá mới nhất xếp lên đầu
                         .ToList();
            }
        }
    }
}
