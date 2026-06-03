using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyAmThucNhaTrang.DAL
{
    // Đã đổi 'internal' thành 'public' để BLL có thể truy cập
    public class DanhMucDAL
    {
        // Hàm lấy toàn bộ danh sách danh mục
        public List<DANHMUC> GetAll()
        {
            // QuanLyAmThucNhaTrangEntities là tên DbContext được EF sinh ra
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Lấy tất cả dữ liệu trong bảng DANHMUC và trả về dưới dạng List
                return db.DANHMUC.ToList();
            }
        }
    }
}
