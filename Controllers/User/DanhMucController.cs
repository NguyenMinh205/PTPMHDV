using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    public class DanhMucController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        public DanhMucController()
        {
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
        }

        // 1. Lấy tất cả danh mục
        [HttpGet]
        [Route("api/danhmuc/all")]
        public IEnumerable<DANHMUC> GetAll()
        {
            return db.DANHMUCs;
        }

        // 2. Lấy danh mục theo ID
        [HttpGet]
        [Route("api/danhmuc/{ma}")]
        public DANHMUC GetById(string ma)
        {
            return db.DANHMUCs.Find(ma);
        }


        // 3. Tìm danh mục theo từ khóa
        [HttpGet]
        [Route("api/danhmuc/search/{key}")]
        public List<DANHMUC> Search(string key)
        {
            return db.DANHMUCs
                .Where(x => x.TenDM.Contains(key))
                .ToList();
        }


        // 4. Lấy top N danh mục
        [HttpGet]
        [Route("api/danhmuc/top/{n}")]
        public List<DANHMUC> GetTop(int n)
        {
            return db.DANHMUCs.Take(n).ToList();
        }


        // 5. Đếm số sản phẩm theo danh mục
        [HttpGet]
        [Route("api/danhmuc/count/{ma}")]
        public int CountProducts(string ma)
        {
            return db.SANPHAMs.Count(x => x.MaDM == ma);
        }


        // 6. Danh mục có sản phẩm không?
        [HttpGet]
        [Route("api/danhmuc/has/{ma}")]
        public bool HasProducts(string ma)
        {
            return db.SANPHAMs.Any(x => x.MaDM == ma);
        }


        // 7. Lấy danh sách sản phẩm theo danh mục
        [HttpGet]
        [Route("api/danhmuc/products/{ma}")]
        public List<SANPHAM> GetProductsByDM(string ma)
        {
            return db.SANPHAMs.Where(x => x.MaDM == ma).ToList();
        }


        // 8. Lấy danh mục liên quan (demo)
        [HttpGet]
        [Route("api/danhmuc/related/{ma}")]
        public List<DANHMUC> Related(string ma)
        {
            return db.DANHMUCs.Where(x => x.MaDM != ma).Take(3).ToList();
        }


        // 9. Lọc danh mục theo độ dài tên
        [HttpGet]
        [Route("api/danhmuc/length/{len}")]
        public List<DANHMUC> FilterByLength(int len)
        {
            return db.DANHMUCs.Where(x => x.TenDM.Length >= len).ToList();
        }


        // 10. Danh mục mô tả chứa từ khóa
        [HttpGet]
        [Route("api/danhmuc/filter/{key}")]
        public List<DANHMUC> FilterByDesc(string key)
        {
            return db.DANHMUCs.Where(x => x.MoTa.Contains(key)).ToList();
        }
    }
}
