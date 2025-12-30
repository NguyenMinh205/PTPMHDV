using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    [RoutePrefix("api/CustomerOrders")]
    public class CustomerOrdersController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        public CustomerOrdersController()
        {
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
        }

        //1 lấy tất cả đơn hàng và tổng tiền
        [HttpGet]
        [Route("LayDonHangCuaToi")]
        public IEnumerable<object> LayDonHangCuaToi(string userId)
        {
            var query = from dh in db.DONHANGs
                        join ct in db.CHITIETDONHANGs
                        on dh.MaDonHang equals ct.MaDonHang
                        where dh.MaND == userId
                        group ct by new
                        {
                            dh.MaDonHang,
                            dh.NgayDatHang,
                            dh.TrangThai
                        } into g
                        select new
                        {
                            MaDonHang = g.Key.MaDonHang,
                            NgayDat = g.Key.NgayDatHang,
                            TrangThai = g.Key.TrangThai,
                            TongTien = g.Sum(x => x.SoLuong * x.GiaBan)
                        };
            return query.ToList();
        }
        
        //2 lấy đơn hàng theo mã
        [HttpGet]
        [Route("LayDonHangTheoMa")]
        public DONHANG LayDonHangTheoMa(string id, string userId)
        {
            DONHANG order = db.DONHANGs.FirstOrDefault(o => o.MaDonHang == id && o.MaND == userId);
            return order;
        }

        //3 lấy chi tiết đơn hàng join 3 bảng
        [HttpGet]
        [Route("LayChiTietDonHang")]
        public IEnumerable<object> LayChiTietDonHang(string id, string userId)
        {
            var query = from dh in db.DONHANGs
                        join ct in db.CHITIETDONHANGs on dh.MaDonHang equals ct.MaDonHang
                        join sp in db.SANPHAMs on ct.MaSP equals sp.MaSP
                        where dh.MaDonHang == id && dh.MaND == userId
                        select new
                        {
                            MaSP = sp.MaSP,
                            TenSP = sp.TenSP,
                            SoLuong = ct.SoLuong,
                            GiaBan = ct.GiaBan,
                            ThanhTien = ct.SoLuong * ct.GiaBan
                        };
            return query.ToList();
        }

        //4 lấy đơn hàng theo trạng thái
        [HttpGet]
        [Route("LayDonHangTheoTrangThai")]
        public IEnumerable<DONHANG> LayDonHangTheoTrangThai(string status, string userId)
        {
            IEnumerable<DONHANG> query = db.DONHANGs.Where(o => o.TrangThai == status && o.MaND == userId);
            return query;
        }

        // 5. Lọc đơn hàng từ ngày... đến ngày...
        [HttpGet]
        [Route("LayDonHangTheoKhoangThoiGian")]
        public IEnumerable<object> LayDonHangTheoKhoangThoiGian(string userId, DateTime tuNgay, DateTime denNgay)
        {

            DateTime denNgayCuoi = denNgay.AddDays(1).AddSeconds(-1);

            var query = from dh in db.DONHANGs
                        where dh.MaND == userId
                           && dh.NgayDatHang >= tuNgay
                           && dh.NgayDatHang <= denNgayCuoi
                        select new
                        {
                            MaDonHang = dh.MaDonHang,
                            NgayDat = dh.NgayDatHang,
                            TrangThai = dh.TrangThai,
                            TongTien = dh.TongTien,
                            DiaChi = dh.DiaChiGiaoHang
                        };
            return query.OrderByDescending(x => x.NgayDat).ToList();
        }

        // 6. Tra cứu đơn hàng  (Dùng Mã đơn + SĐT nhận hàng)
        [HttpGet]
        [Route("TraCuuDonHangGuest")]
        public object TraCuuDonHangGuest(string maDonHang, string sdtNhanHang)
        {
            var donHang = db.DONHANGs
                .Where(d => d.MaDonHang == maDonHang && d.SDTNhanHang == sdtNhanHang)
                .Select(d => new
                {
                    MaDonHang = d.MaDonHang,
                    NgayDat = d.NgayDatHang,
                    TrangThai = d.TrangThai,
                    NguoiNhan = d.SDTNhanHang,
                    DiaChi = d.DiaChiGiaoHang,
                    TongTien = d.TongTien,

                    SanPham = d.CHITIETDONHANGs.Select(ct => new
                    {
                        TenSP = ct.SANPHAM.TenSP,
                        SoLuong = ct.SoLuong
                    })
                })
                .FirstOrDefault();

            return donHang;
        }

        // 7. Lấy top N đơn hàng mới nhất (Ví dụ: 5 đơn gần đây)
        [HttpGet]
        [Route("LayDonHangMoiNhat")]
        public IEnumerable<object> LayDonHangMoiNhat(string userId, int soLuong = 5)
        {
            var query = db.DONHANGs
                .Where(d => d.MaND == userId)
                .OrderByDescending(d => d.NgayDatHang)
                .Take(soLuong)
                .Select(d => new
                {
                    MaDonHang = d.MaDonHang,
                    NgayDat = d.NgayDatHang,
                    TrangThai = d.TrangThai,
                    TongTien = d.TongTien
                });

            return query.ToList();
        }

        // 8. Khách hàng tự hủy đơn hàng
        [HttpPost]
        [Route("HuyDonHang")]
        public IHttpActionResult HuyDonHang(string maDonHang, string userId)
        {
            var donHang = db.DONHANGs.FirstOrDefault(d => d.MaDonHang == maDonHang && d.MaND == userId);

            if (donHang == null) return NotFound();


            if (donHang.TrangThai == "Đang xử lý")
            {
                donHang.TrangThai = "Đã hủy";
                db.SaveChanges();
                return Ok("Đã hủy đơn hàng thành công");
            }
            else
            {
                return BadRequest("Đơn hàng đã được xử lý hoặc đang giao, không thể hủy.");
            }
        }
    }
}
