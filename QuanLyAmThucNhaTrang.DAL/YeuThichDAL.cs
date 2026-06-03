using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace QuanLyAmThucNhaTrang.DAL
{
    public class YeuThichDAL
    {
        // 1. Kiểm tra xem người dùng đã lưu địa điểm này chưa (Đáp ứng QĐ4)
        public bool KiemTraDaLuu(int maTK, int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.YEUTHICH.Any(y => y.MaTK == maTK && y.MaDD == maDD);
            }
        }

        // 2. Thêm mới một bản ghi yêu thích
        public bool ThemYeuThich(int maTK, int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var ytMoi = new YEUTHICH
                    {
                        MaTK = maTK,
                        MaDD = maDD,
                        NgayLuu = DateTime.Now
                    };
                    db.YEUTHICH.Add(ytMoi);
                    db.SaveChanges();
                    return true;
                }
                catch { return false; }
            }
        }

        // 3. Xóa bỏ bản ghi yêu thích (Hủy lưu)
        public bool XoaYeuThich(int maTK, int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var yt = db.YEUTHICH.FirstOrDefault(y => y.MaTK == maTK && y.MaDD == maDD);
                    if (yt != null)
                    {
                        db.YEUTHICH.Remove(yt);
                        db.SaveChanges();
                    }
                    return true;
                }
                catch { return false; }
            }
        }

        // 4. Kéo danh sách các địa điểm đã lưu của một user (Dùng cho Trang Yêu Thích)
        public List<YEUTHICH> LayDanhSachYeuThich(int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                return db.YEUTHICH
                         .Include(y => y.DIADIEM) // Kéo theo thông tin quán ăn
                         .Include(y => y.DIADIEM.DANHMUC) // Kéo theo tên danh mục
                         .Include(y => y.DIADIEM.HINHANH) // Kéo theo ảnh mặt tiền
                         .Where(y => y.MaTK == maTK)
                         .OrderByDescending(y => y.NgayLuu) // Cái nào mới lưu thì xếp lên đầu
                         .ToList();
            }
        }
    }
}
