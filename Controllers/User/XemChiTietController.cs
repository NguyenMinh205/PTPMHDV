using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    public class XemChiTietController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        // =================================================
        // 1. Xem chi tiết sản phẩm
        // GET: api/product/detail/SP001
        // =================================================
        [HttpGet]
        [Route("api/product/detail/{id}")]
        public object GetProductDetail(string id)
        {
            // Lấy sản phẩm
            var sp = db.SANPHAMs.FirstOrDefault(p => p.MaSP == id);
            if (sp == null)
                return null;

            // Lấy tên danh mục
            var tenDM = db.DANHMUCs
                          .Where(d => d.MaDM == sp.MaDM)
                          .Select(d => d.TenDM)
                          .FirstOrDefault();

            // Lấy danh sách ảnh
            var listAnh = db.HINHANHs
                            .Where(h => h.MaSP == id)
                            .Select(h => new
                            {
                                h.MaAnh,
                                h.KichThuoc
                            })
                            .ToList();

            // Trả dữ liệu chi tiết
            return new
            {
                sp.MaSP,
                sp.TenSP,
                sp.GiaBan,
                sp.AnhMinhHoa,
                sp.MoTa,
                sp.ThongSo,
                sp.TrangThai,
                sp.MaDM,
                TenDanhMuc = tenDM,
                DanhSachAnh = listAnh
            };
        }

        // =================================================
        // 2. Lấy sản phẩm liên quan
        // GET: api/product/related/SP001
        // =================================================
        [HttpGet]
        [Route("api/product/related/{id}")]
        public List<object> GetRelatedProducts(string id)
        {
            // Lấy sản phẩm hiện tại
            var currentProduct = db.SANPHAMs.FirstOrDefault(p => p.MaSP == id);
            if (currentProduct == null)
                return new List<object>();

            // Lấy sản phẩm cùng danh mục
            var list = db.SANPHAMs
                         .Where(p => p.MaDM == currentProduct.MaDM
                                  && p.MaSP != id
                                  && p.TrangThai == "Còn hàng")
                         .Take(4)
                         .Select(p => new
                         {
                             p.MaSP,
                             p.TenSP,
                             p.GiaBan,
                             p.AnhMinhHoa
                         })
                         .ToList<object>();

            return list;
        }
    }
}
