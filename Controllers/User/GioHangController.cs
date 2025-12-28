using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    public class GioHangController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        // --------------------------------------------------
        // 1. Thêm sản phẩm vào giỏ hàng
        // POST: api/cart/add/ND001/SP001/1
        // --------------------------------------------------
        [HttpPost]
        [Route("api/cart/add/{userId}/{productId}/{quantity}")]
        public string AddToCart(string userId, string productId, int quantity)
        {
            // Kiểm tra sản phẩm
            var product = db.SANPHAMs.FirstOrDefault(p => p.MaSP == productId);
            if (product == null)
                return "Sản phẩm không tồn tại";

            // Lấy giỏ hàng của user
            var cart = db.GIOHANGs.FirstOrDefault(g => g.MaND == userId);
            if (cart == null)
                return "Người dùng chưa có giỏ hàng";

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var item = db.CHITIETGIOHANGs
                         .FirstOrDefault(c => c.MaGH == cart.MaGH && c.MaSP == productId);

            if (item != null)
            {
                // Nếu có rồi → cộng thêm số lượng
                item.SoLuong += quantity;
            }
            else
            {
                // Nếu chưa có → thêm mới
                db.CHITIETGIOHANGs.Add(new CHITIETGIOHANG
                {
                    MaGH = cart.MaGH,
                    MaSP = productId,
                    SoLuong = quantity
                });
            }

            db.SaveChanges();
            return "Thêm sản phẩm vào giỏ hàng thành công";
        }

        // --------------------------------------------------
        // 2. Xem giỏ hàng theo người dùng
        // GET: api/cart/view?userId=ND001
        // --------------------------------------------------
        [HttpGet]
        [Route("api/cart/view")]
        public List<object> ViewCart(string userId)
        {
            var cart = db.GIOHANGs.FirstOrDefault(g => g.MaND == userId);
            if (cart == null)
                return new List<object>();

            var list = (from ct in db.CHITIETGIOHANGs
                        join sp in db.SANPHAMs on ct.MaSP equals sp.MaSP
                        where ct.MaGH == cart.MaGH
                        select new
                        {
                            sp.MaSP,
                            sp.TenSP,
                            sp.GiaBan,
                            sp.AnhMinhHoa,
                            ct.SoLuong,
                            ThanhTien = ct.SoLuong * sp.GiaBan
                        }).ToList<object>();

            return list;
        }

        // --------------------------------------------------
        // 3. Cập nhật số lượng sản phẩm trong giỏ
        // PUT: api/cart/update?userId=ND001&productId=SP001&quantity=3
        // --------------------------------------------------
        [HttpPut]
        [Route("api/cart/update")]
        public string UpdateQuantity(string userId, string productId, int quantity)
        {
            var cart = db.GIOHANGs.FirstOrDefault(g => g.MaND == userId);
            if (cart == null)
                return "Không tìm thấy giỏ hàng";

            var item = db.CHITIETGIOHANGs
                         .FirstOrDefault(c => c.MaGH == cart.MaGH && c.MaSP == productId);

            if (item == null)
                return "Sản phẩm không tồn tại trong giỏ";

            item.SoLuong = quantity;
            db.SaveChanges();

            return "Cập nhật số lượng thành công";
        }

        // --------------------------------------------------
        // 4. Xóa 1 sản phẩm khỏi giỏ hàng
        // DELETE: api/cart/delete?userId=ND001&productId=SP001
        // --------------------------------------------------
        [HttpDelete]
        [Route("api/cart/delete")]
        public string RemoveFromCart(string userId, string productId)
        {
            var cart = db.GIOHANGs.FirstOrDefault(g => g.MaND == userId);
            if (cart == null)
                return "Không tìm thấy giỏ hàng";

            var item = db.CHITIETGIOHANGs
                         .FirstOrDefault(c => c.MaGH == cart.MaGH && c.MaSP == productId);

            if (item == null)
                return "Sản phẩm không có trong giỏ";

            db.CHITIETGIOHANGs.Remove(item);
            db.SaveChanges();

            return "Xóa sản phẩm khỏi giỏ hàng thành công";
        }

        // --------------------------------------------------
        // 5. Xóa toàn bộ giỏ hàng
        // DELETE: api/cart/clear/ND001
        // --------------------------------------------------
        [HttpDelete]
        [Route("api/cart/clear/{userId}")]
        public string ClearCart(string userId)
        {
            var cart = db.GIOHANGs.FirstOrDefault(g => g.MaND == userId);
            if (cart == null)
                return "Không tìm thấy giỏ hàng";

            var items = db.CHITIETGIOHANGs
                          .Where(c => c.MaGH == cart.MaGH)
                          .ToList();

            db.CHITIETGIOHANGs.RemoveRange(items);
            db.SaveChanges();

            return "Đã xóa toàn bộ giỏ hàng";
        }
    }
}
