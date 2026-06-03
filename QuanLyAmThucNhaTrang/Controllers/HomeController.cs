using QuanLyAmThucNhaTrang.BLL;
using QuanLyAmThucNhaTrang.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
namespace QuanLyAmThucNhaTrang.Controllers
{
    public class HomeController : Controller
    {
        private DiaDiemBLL _diaDiemBLL = new DiaDiemBLL();

        public ActionResult Index()
        {
            using (var db = new QuanLyAmThucNhaTrangEntities())
            {
                // Thêm .Include() để "gói" sẵn dữ liệu Danh mục và Hình ảnh đem ra View
                var dsNoiBat = db.DIADIEM.Include(d => d.DANHMUC)
                                         .Include(d => d.HINHANH)
                                         .Where(d => d.TrangThai == "DangHoatDong")
                                         .OrderByDescending(d => d.DiemDanhGiaTB)
                                         .Take(6)
                                         .ToList();

                return View(dsNoiBat);
            }
        }
    }
}