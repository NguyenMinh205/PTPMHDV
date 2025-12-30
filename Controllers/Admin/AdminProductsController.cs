using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    [RoutePrefix("api/AdminProducts")]
    public class AdminProductsController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        public AdminProductsController()
        {
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
        }

        //9 lấy toàn bộ sản phẩm quản trị
        [HttpGet]
        [Route("LayToanBoSanPham")]
        public IEnumerable<SANPHAM> LayToanBoSanPham()
        {
            IEnumerable<SANPHAM> query = db.SANPHAMs;
            return query;
        }

        //10 lấy sản phẩm theo mã
        [HttpGet]
        [Route("LaySanPhamTheoMa")]
        public SANPHAM LaySanPhamTheoMa(string id)
        {
            SANPHAM sp = db.SANPHAMs.FirstOrDefault(x => x.MaSP == id);
            return sp;
        }

        //11 thêm sản phẩm
        [HttpPost]
        [Route("ThemSanPham")]
        public bool ThemSanPham(string ma, string ten, decimal gia, string madm, string anh = null, string mota = null, string thongso = null, string trangthai = null)
        {
            SANPHAM sp = db.SANPHAMs.FirstOrDefault(x => x.MaSP == ma);
            if (sp == null)
            {
                SANPHAM newsp = new SANPHAM();
                newsp.MaSP = ma;
                newsp.TenSP = ten;
                newsp.GiaBan = gia;
                newsp.MaDM = madm;
                newsp.AnhMinhHoa = anh;
                newsp.MoTa = mota;
                newsp.ThongSo = thongso;
                newsp.TrangThai = trangthai;
                db.SANPHAMs.Add(newsp);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        //12 sửa sản phẩm
        [HttpPut]
        [Route("SuaSanPham")]
        public bool SuaSanPham(string ma, string ten, decimal gia, string madm, string anh = null, string mota = null, string thongso = null, string trangthai = null)
        {
            SANPHAM sp = db.SANPHAMs.FirstOrDefault(x => x.MaSP == ma);
            if (sp != null)
            {
                sp.TenSP = ten;
                sp.GiaBan = gia;
                sp.MaDM = madm;
                sp.AnhMinhHoa = anh;
                sp.MoTa = mota;
                sp.ThongSo = thongso;
                sp.TrangThai = trangthai;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        //13 xóa sản phẩm
        [HttpDelete]
        [Route("XoaSanPham")]
        public bool XoaSanPham(string ma)
        {
            SANPHAM sp = db.SANPHAMs.FirstOrDefault(x => x.MaSP == ma);
            if (sp != null)
            {
                db.SANPHAMs.Remove(sp);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        //14 lấy sản phẩm theo danh mục
        [HttpGet]
        [Route("LaySanPhamTheoDanhMuc")]
        public IEnumerable<SANPHAM> LaySanPhamTheoDanhMuc(string madm)
        {
            IEnumerable<SANPHAM> query = db.SANPHAMs.Where(x => x.MaDM == madm);
            return query;
        }

        //15 lấy sản phẩm theo tên
        [HttpGet]
        [Route("TimKiemSanPhamTheoTen")]
        public IEnumerable<SANPHAM> TimKiemSanPhamTheoTen(string ten)
        {
            IEnumerable<SANPHAM> query = db.SANPHAMs.Where(x => x.TenSP.Contains(ten));
            return query;
        }

        //16 thống kê sản phẩm bán chạy
        [HttpGet]
        [Route("ThongKeSanPhamBanChay")]
        public IEnumerable<object> ThongKeSanPhamBanChay()
        {
            var query = from ct in db.CHITIETDONHANGs
                        join sp in db.SANPHAMs on ct.MaSP equals sp.MaSP
                        group ct by new { sp.MaSP, sp.TenSP } into g
                        select new
                        {
                            g.Key.MaSP,
                            g.Key.TenSP,
                            TongSoLuong = g.Sum(x => x.SoLuong)
                        };
            return query.OrderByDescending(x => x.TongSoLuong).ToList();
        }
    }
}
