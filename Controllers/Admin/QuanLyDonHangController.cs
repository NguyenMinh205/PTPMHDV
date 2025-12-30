using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    public class QuanLyDonHangController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        public QuanLyDonHangController()
        {
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
        }

        // 1. Lấy tất cả đơn hàng
        [HttpGet]
        [Route("api/donhang/all")]
        public IEnumerable<DONHANG> GetAll()
        {
            return db.DONHANGs.OrderByDescending(x => x.NgayDatHang).ToList();
        }

        // 2. Tìm kiếm theo Mã Đơn Hàng
        [HttpGet]
        [Route("api/donhang/search/id/{maDH}")]
        public List<DONHANG> SearchByMaDH(string maDH)
        {
            return db.DONHANGs.Where(x => x.MaDonHang.Contains(maDH)).ToList();
        }

        // 3. Tìm kiếm theo Số Điện Thoại
        [HttpGet]
        [Route("api/donhang/search/phone/{sdt}")]
        public List<DONHANG> SearchBySDT(string sdt)
        {
            return db.DONHANGs.Where(x => x.SDTNhanHang.Contains(sdt)).ToList();
        }   

        //4. Tìm kiếm theo Mã Người Dùng
        [HttpGet]
        [Route("api/donhang/search/user/{maND}")]
        public List<DONHANG> SearchByMaND(string maND)
        {
            return db.DONHANGs.Where(x => x.MaND.Contains(maND)).ToList();
        }

        // 5. Tìm kiếm theo Ngày (dd-MM-yyyy)
        [HttpGet]
        [Route("api/donhang/search/date/{dateStr}")]
        public IHttpActionResult SearchByDate(string dateStr)
        {
            DateTime dateValue;
            bool isValid = DateTime.TryParseExact(dateStr, new[] { "dd-MM-yyyy", "dd/MM/yyyy" },
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateValue);

            if (!isValid) return Content(HttpStatusCode.BadRequest, "Sai định dạng ngày (dd-MM-yyyy)");

            var result = db.DONHANGs
                .Where(x => DbFunctions.TruncateTime(x.NgayDatHang) == dateValue.Date)
                .ToList();
            return Ok(result);
        }

        // 6. Chi tiết đơn hàng
        [HttpGet]
        [Route("api/donhang/detail/{maDH}")]
        public IHttpActionResult GetDetail(string maDH)
        {
            var donHang = db.DONHANGs.Include("CHITIETDONHANGs.SANPHAM")
                            .Include(d => d.NGUOIDUNG) 
                            .FirstOrDefault(x => x.MaDonHang == maDH);

            if (donHang == null) return NotFound();

            if (donHang.NGUOIDUNG != null)
            {
                donHang.NGUOIDUNG.DONHANGs = null;
                donHang.NGUOIDUNG.GIOHANGs = null;
            }

            if (donHang.CHITIETDONHANGs != null)
            {
                foreach (var chitiet in donHang.CHITIETDONHANGs)
                {
                    chitiet.DONHANG = null;

                    if (chitiet.SANPHAM != null)
                    {
                        chitiet.SANPHAM.CHITIETDONHANGs = null;
                        chitiet.SANPHAM.CHITIETGIOHANGs = null;
                        chitiet.SANPHAM.DANHMUC = null;
                        chitiet.SANPHAM.HINHANHs = null;
                    }
                }
            }
            return Ok(donHang);
        }

        // 7. Cập nhật trạng thái
        [HttpPost]
        [Route("api/donhang/update-status")]
        public IHttpActionResult UpdateStatus(string maDH, string trangThaiMoi)
        {
            var donHang = db.DONHANGs.Find(maDH);
            if (donHang == null) return NotFound();

            if (donHang.TrangThai == "Đã hủy" || donHang.TrangThai == "Hoàn thành")
                return Content(HttpStatusCode.BadRequest, "Đơn hàng đã đóng, không thể sửa.");

            donHang.TrangThai = trangThaiMoi;
            db.SaveChanges();
            return Ok("Cập nhật thành công");
        }

        // 8. Hủy đơn hàng
        [HttpPost]
        [Route("api/donhang/cancel/{maDH}")]
        public IHttpActionResult CancelOrder(string maDH)
        {
            var donHang = db.DONHANGs.Include(d => d.CHITIETDONHANGs).FirstOrDefault(x => x.MaDonHang == maDH);
            if (donHang == null) return NotFound();
            if (donHang.TrangThai == "Đã hủy") return Content(HttpStatusCode.BadRequest, "Đã hủy rồi.");

            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    donHang.TrangThai = "Đã hủy";
                    foreach (var ct in donHang.CHITIETDONHANGs)
                    {
                        var sp = db.SANPHAMs.Find(ct.MaSP);
                        if (sp != null) sp.SoLuongTon = (sp.SoLuongTon ?? 0) + ct.SoLuong;
                    }
                    db.SaveChanges();
                    trans.Commit();
                    return Ok("Đã hủy và hoàn kho.");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return InternalServerError(ex);
                }
            }
        }

        // 9. Thống kê Doanh thu
        [HttpGet]
        [Route("api/thongke/doanhthu")]
        public decimal GetTotalRevenue()
        {
            return db.DONHANGs.Where(x => x.TrangThai == "Đã giao" || x.TrangThai == "Hoàn thành")
                .Sum(x => (decimal?)x.TongTien) ?? 0;
        }

        //10. Thống kê số lượng đơn hàng theo trạng thái
        [HttpGet]
        [Route("api/thongke/count-status")]
        public IHttpActionResult GetOrderCounts()
        {
            var data = db.DONHANGs.GroupBy(x => x.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() }).ToList();
            return Ok(data);
        }

        //11. Lịch sử mua hàng theo Mã Người Dùng
        [HttpGet]
        [Route("api/taikhoan/{maND}/orders")]
        public IEnumerable<DONHANG> GetOrdersByUser(string maND)
        {
            return db.DONHANGs.Where(x => x.MaND == maND).OrderByDescending(x => x.NgayDatHang).ToList();
        }
    }
}
