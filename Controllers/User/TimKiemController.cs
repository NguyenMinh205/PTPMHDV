using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    public class TimKiemController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();


        // 1. Lấy tất cả sản phẩm
        [HttpGet]
        [Route("api/products/getall")]
        public List<SANPHAM> GetAll()
        {
            return db.SANPHAMs.ToList();
        }

        // 2. Tìm sản phẩm theo tên
        // Ví dụ: api/products/search-name?keyword=Laptop MSI Creator Z16
        [HttpGet]
        [Route("api/products/search-name")]
        public List<SANPHAM> SearchByName(string keyword)
        {
            return db.SANPHAMs
                     .Where(p => p.TenSP.Contains(keyword))
                     .ToList();
        }

        // 3. Tìm sản phẩm theo danh mục
        // Ví dụ: api/products/by-category?categoryId=DM01
        [HttpGet]
        [Route("api/products/by-category")]
        public List<SANPHAM> SearchByCategory(string categoryId)
        {
            return db.SANPHAMs
                     .Where(p => p.MaDM == categoryId)
                     .ToList();
        }

        // 4. Tìm sản phẩm theo khoảng giá
        // Ví dụ: api/products/by-price?min=1000000&max=5000000
        [HttpGet]
        [Route("api/products/by-price")]
        public List<SANPHAM> SearchByPrice(decimal min, decimal max)
        {
            return db.SANPHAMs
                     .Where(p => p.GiaBan >= min && p.GiaBan <= max)
                     .ToList();
        }

        // 5. Tìm sản phẩm theo mã sản phẩm (xem chi tiết)
        // Ví dụ: api/products/find?id=SP001
        [HttpGet]
        [Route("api/products/find")]
        public SANPHAM FindProduct(string id)
        {
            return db.SANPHAMs.FirstOrDefault(p => p.MaSP == id);
        }

        // 6. Tìm sản phẩm theo trạng thái
        // Ví dụ: api/products/by-status?status=Còn hàng
        [HttpGet]
        [Route("api/products/by-status")]
        public List<SANPHAM> SearchByStatus(string status)
        {
            return db.SANPHAMs
                     .Where(p => p.TrangThai == status)
                     .ToList();
        }

        // 7. Tìm sản phẩm mới nhất
        [HttpGet]
        [Route("api/products/latest")]
        public SANPHAM GetLatestProduct()
        {
            return db.SANPHAMs
                     .OrderByDescending(p => p.MaSP)
                     .FirstOrDefault();
        }

        // 8.Tìm Top 10 sản phẩm giá cao nhất
        [HttpGet]
        [Route("api/products/top10")]
        public List<SANPHAM> Top10Products()
        {
            return db.SANPHAMs
                     .OrderByDescending(p => p.GiaBan)
                     .Take(10)
                     .ToList();
        }
    }
}
