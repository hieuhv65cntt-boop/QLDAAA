using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyAmThucNhaTrang.DAL;

namespace QuanLyAmThucNhaTrang.BLL
{
    public class DanhMucBLL
    {
        private DanhMucDAL _danhMucDAL = new DanhMucDAL();

        public List<DANHMUC> GetAll()
        {
            // Gọi hàm GetAll từ DAL
            return _danhMucDAL.GetAll();
        }
    }
}
