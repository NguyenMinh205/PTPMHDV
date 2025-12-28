using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    [RoutePrefix("api/order")]
    public class OrderController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        // 2.3.9.1 Tạo đơn hàng
        // POST: api/order/create
        [HttpPost]
        [Route("create")]
        public IHttpActionResult CreateOrder(string maND, string diaChi, string sdt)
        {
            // 1. Kiểm tra giỏ hàng
            var cart = db.GIOHANGs.FirstOrDefault(x => x.MaND == maND);
            if (cart == null)
                return BadRequest("Giỏ hàng trống");

            var cartItems = db.CHITIETGIOHANGs
                .Where(x => x.MaGH == cart.MaGH)
                .ToList();

            if (!cartItems.Any())
                return BadRequest("Không có sản phẩm để đặt hàng");

            // 2. TÍNH TỔNG TIỀN (GiaBan là decimal, SoLuong là int?)
            decimal tongTien = (from ct in db.CHITIETGIOHANGs
                                join sp in db.SANPHAMs on ct.MaSP equals sp.MaSP
                                where ct.MaGH == cart.MaGH
                                select (ct.SoLuong ?? 0) * sp.GiaBan
                               ).DefaultIfEmpty(0).Sum();

            // 3. Tạo đơn hàng
            var donHang = new DONHANG
            {
                MaDonHang = "DH" + DateTime.Now.Ticks,
                MaND = maND,
                NgayDatHang = DateTime.Now,
                TrangThai = "Đang xử lý",
                DiaChiGiaoHang = diaChi,
                SDTNhanHang = sdt,
                TongTien = tongTien
            };

            db.DONHANGs.Add(donHang);

            // 4. Tạo chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                decimal giaBan = db.SANPHAMs
                    .Where(x => x.MaSP == item.MaSP)
                    .Select(x => x.GiaBan)
                    .First();

                db.CHITIETDONHANGs.Add(new CHITIETDONHANG
                {
                    MaCTDH = Guid.NewGuid().ToString("N").Substring(0, 8),
                    MaDonHang = donHang.MaDonHang,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong ?? 0,
                    GiaBan = giaBan
                });
            }

            // 5. Xóa giỏ hàng sau khi đặt
            db.CHITIETGIOHANGs.RemoveRange(cartItems);
            db.SaveChanges();

            return Ok(new
            {
                message = "Đặt hàng thành công",
                donHang.MaDonHang,
                donHang.TongTien
            });
        }

        // 2.3.9.2 Xem danh sách đơn hàng
        // GET: api/order/list/ND001
        [HttpGet]
        [Route("list/{maND}")]
        public IHttpActionResult GetOrders(string maND)
        {
            var orders = db.DONHANGs
                .Where(x => x.MaND == maND)
                .Select(x => new
                {
                    x.MaDonHang,
                    x.NgayDatHang,
                    x.TrangThai,
                    x.TongTien
                })
                .ToList();

            return Ok(orders);
        }
    }
}
