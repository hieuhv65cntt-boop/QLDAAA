using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace QuanLyAmThucNhaTrang.DAL
{
    public class KhuyenMaiDAL
    {
        // Lấy danh sách Khuyến mãi thuộc các quán của 1 Chủ sở hữu
        public List<KHUYENMAI> LayDanhSachKhuyenMai(int maTK)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Lọc KM dựa trên MaTK của bảng DIADIEM
                return db.KHUYENMAI.Include(k => k.DIADIEM)
                         .Where(k => k.DIADIEM.MaTK == maTK)
                         .OrderByDescending(k => k.NgayBatDau)
                         .ToList();
            }
        }

        public bool ThemKhuyenMai(KHUYENMAI km)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try { db.KHUYENMAI.Add(km); db.SaveChanges(); return true; }
                catch { return false; }
            }
        }

        public bool XoaKhuyenMai(int maKM)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                try
                {
                    var km = db.KHUYENMAI.Find(maKM);
                    if (km != null) { db.KHUYENMAI.Remove(km); db.SaveChanges(); }
                    return true;
                }
                catch { return false; }
            }
        }
        public List<KHUYENMAI> LayKhuyenMaiHieuLuc(int maDD)
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                var homNay = System.DateTime.Now.Date;

                return db.KHUYENMAI
                         .Where(k => k.MaDD == maDD
                                  && k.TrangThai == "ConHieuLuc"
                                  && k.NgayBatDau <= homNay
                                  && k.NgayKetThuc >= homNay)
                         .OrderBy(k => k.NgayKetThuc) // Ưu đãi nào sắp hết hạn thì xếp lên đầu
                         .ToList();
            }
        }
    }
}
