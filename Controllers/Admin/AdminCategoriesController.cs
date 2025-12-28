using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    public class AdminCategoriesController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        //17 lấy toàn bộ danh mục
        [HttpGet]
        [Route("LayToanBoDanhMuc")]
        public IEnumerable<DANHMUC> LayToanBoDanhMuc()
        {
            IEnumerable<DANHMUC> query = db.DANHMUCs;
            return query;
        }

        //18 lấy danh mục theo mã
        [HttpGet]
        [Route("LayDanhMucTheoMa")]
        public DANHMUC LayDanhMucTheoMa(string id)
        {
            DANHMUC dm = db.DANHMUCs.FirstOrDefault(x => x.MaDM == id);
            return dm;
        }

        //19 thêm mới danh mục
        [HttpPost]
        [Route("ThemDanhMuc")]
        public bool ThemDanhMuc(string ma, string ten, string mota = null)
        {

            DANHMUC dm = db.DANHMUCs.FirstOrDefault(x => x.MaDM == ma);
            if (dm == null)
            {
                DANHMUC newDm = new DANHMUC();
                newDm.MaDM = ma;
                newDm.TenDM = ten;
                newDm.MoTa = mota;

                db.DANHMUCs.Add(newDm);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        //20 sửa danh mục
        [HttpPut]
        [Route("SuaDanhMuc")]
        public bool SuaDanhMuc(string ma, string ten, string mota = null)
        {
            DANHMUC dm = db.DANHMUCs.FirstOrDefault(x => x.MaDM == ma);
            if (dm != null)
            {
                dm.TenDM = ten;
                dm.MoTa = mota;

                db.SaveChanges();
                return true;
            }
            return false;
        }

        //21 xóa danh mục theo mã
        [HttpDelete]
        [Route("XoaDanhMuc")]
        public bool XoaDanhMuc(string ma)
        {
            DANHMUC dm = db.DANHMUCs.FirstOrDefault(x => x.MaDM == ma);
            if (dm != null)
            {

                bool coSanPham = db.SANPHAMs.Any(x => x.MaDM == ma);
                if (coSanPham)
                {
                    return false;
                }

                db.DANHMUCs.Remove(dm);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        //22 tìm kiếm danh mục theo tên
        [HttpGet]
        [Route("TimKiemDanhMucTheoTen")]
        public IEnumerable<DANHMUC> TimKiemDanhMucTheoTen(string ten)
        {
            IEnumerable<DANHMUC> query = db.DANHMUCs.Where(x => x.TenDM.Contains(ten));
            return query;
        }
    }
}
