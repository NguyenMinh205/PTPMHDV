using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    [RoutePrefix("api/cart")]
    public class CartController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        // 2.3.8.1 Xem giỏ hàng
        // GET: api/cart/ND001
        [HttpGet]
        [Route("{maND}")]
        public IHttpActionResult GetCart(string maND)
        {
            var cart = db.GIOHANGs.FirstOrDefault(x => x.MaND == maND);
            if (cart == null)
                return Ok("Giỏ hàng trống");

            var items = db.CHITIETGIOHANGs
                .Where(x => x.MaGH == cart.MaGH)
                .Select(x => new
                {
                    x.MaSP,
                    x.SANPHAM.TenSP,
                    x.SANPHAM.GiaBan,
                    x.SoLuong,
                    ThanhTien = x.SoLuong * x.SANPHAM.GiaBan
                });

            return Ok(items);
        }

        // 2.3.8.2 Thêm sản phẩm vào giỏ
        // POST: api/cart/add
        [HttpPost]
        [Route("add")]
        public IHttpActionResult AddToCart(string maND, string maSP, int soLuong)
        {
            var cart = db.GIOHANGs.FirstOrDefault(x => x.MaND == maND);
            if (cart == null)
            {
                cart = new GIOHANG { MaGH = "GH" + maND, MaND = maND };
                db.GIOHANGs.Add(cart);
            }

            var item = db.CHITIETGIOHANGs
                .FirstOrDefault(x => x.MaGH == cart.MaGH && x.MaSP == maSP);

            if (item != null)
                item.SoLuong += soLuong;
            else
                db.CHITIETGIOHANGs.Add(new CHITIETGIOHANG
                {
                    MaGH = cart.MaGH,
                    MaSP = maSP,
                    SoLuong = soLuong
                });

            db.SaveChanges();
            return Ok("Thêm sản phẩm vào giỏ hàng thành công");
        }

        // 2.3.8.3 Cập nhật số lượng
        // PUT: api/cart/update
        [HttpPut]
        [Route("update")]
        public IHttpActionResult UpdateQuantity(string maGH, string maSP, int soLuong)
        {
            var item = db.CHITIETGIOHANGs
                .FirstOrDefault(x => x.MaGH == maGH && x.MaSP == maSP);

            if (item == null)
                return NotFound();

            item.SoLuong = soLuong;
            db.SaveChanges();

            return Ok("Cập nhật số lượng thành công");
        }

        // 2.3.8.4 Xóa sản phẩm khỏi giỏ
        // DELETE: api/cart/remove/GH001/SP001
        [HttpDelete]
        [Route("remove/{maGH}/{maSP}")]
        public IHttpActionResult RemoveItem(string maGH, string maSP)
        {
            var item = db.CHITIETGIOHANGs
                .FirstOrDefault(x => x.MaGH == maGH && x.MaSP == maSP);

            if (item == null)
                return NotFound();

            db.CHITIETGIOHANGs.Remove(item);
            db.SaveChanges();

            return Ok("Xóa sản phẩm khỏi giỏ hàng thành công");
        }

        // 2.3.8.5 Xóa toàn bộ giỏ hàng
        // DELETE: api/cart/clear/GH001
        [HttpDelete]
        [Route("clear/{maGH}")]
        public IHttpActionResult ClearCart(string maGH)
        {
            var items = db.CHITIETGIOHANGs.Where(x => x.MaGH == maGH);
            db.CHITIETGIOHANGs.RemoveRange(items);
            db.SaveChanges();

            return Ok("Đã xóa toàn bộ giỏ hàng");
        }
    }
}
