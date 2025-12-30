using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    public class QuanLyTaiKhoanController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        public QuanLyTaiKhoanController()
        {
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
        }

        // 1. Lấy tất cả tài khoản
        [HttpGet]
        [Route("api/taikhoan/all")]
        public IEnumerable<NGUOIDUNG> GetAll()
        {
            return db.NGUOIDUNGs.OrderBy(x => x.TenND).ToList();
        }

        // 2. Chi tiết
        [HttpGet]
        [Route("api/taikhoan/{maND}")]
        public IHttpActionResult GetById(string maND)
        {
            var user = db.NGUOIDUNGs.Find(maND);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // 3. Tìm kiếm
        [HttpGet]
        [Route("api/taikhoan/search/{key}")]
        public List<NGUOIDUNG> Search(string key)
        {
            return db.NGUOIDUNGs
                .Where(x => x.TaiKhoan.Contains(key) || x.TenND.Contains(key) || x.SDT.Contains(key))
                .ToList();
        }

        // 4. Thêm mới
        [HttpPost]
        [Route("api/taikhoan/insert")]
        public IHttpActionResult Insert(NGUOIDUNG model)
        {
            if (db.NGUOIDUNGs.Any(x => x.TaiKhoan == model.TaiKhoan)) return BadRequest("Tài khoản trùng.");
            if (db.NGUOIDUNGs.Any(x => x.MaND == model.MaND)) return BadRequest("Mã trùng.");
            if (db.NGUOIDUNGs.Any(x => x.Email == model.Email)) return BadRequest("Email trùng.");

            if (string.IsNullOrEmpty(model.VaiTro)) model.VaiTro = "User";
            model.TrangThai = true;

            db.NGUOIDUNGs.Add(model);
            db.SaveChanges();
            return Ok("Thêm thành công");
        }

        // 5. Cập nhật
        [HttpPut]
        [Route("api/taikhoan/update")]
        public IHttpActionResult Update(NGUOIDUNG model)
        {
            var user = db.NGUOIDUNGs.Find(model.MaND);
            if (user == null) return NotFound();

            user.TenND = model.TenND;
            user.SDT = model.SDT;
            user.Email = model.Email;
            user.VaiTro = model.VaiTro;
            if (!string.IsNullOrEmpty(model.MatKhau)) user.MatKhau = model.MatKhau;

            db.SaveChanges();
            return Ok("Cập nhật thành công");
        }

        // 6. Xóa
        [HttpDelete]
        [Route("api/taikhoan/delete/{maND}")]
        public IHttpActionResult Delete(string maND)
        {
            var user = db.NGUOIDUNGs.Find(maND);
            if (user == null) return NotFound();
            if (db.DONHANGs.Any(x => x.MaND == maND)) return BadRequest("User này đã có đơn hàng, không thể xóa.");

            db.NGUOIDUNGs.Remove(user);
            db.SaveChanges();
            return Ok("Đã xóa.");
        }

        // 7. Khóa/Mở khóa
        [HttpPost]
        [Route("api/taikhoan/lock/{maND}")]
        public IHttpActionResult ToggleLockState(string maND)
        {
            var user = db.NGUOIDUNGs.Find(maND);
            if (user == null) return NotFound();

            bool currentState = user.TrangThai ?? true;
            user.TrangThai = !currentState;

            db.SaveChanges();
            return Ok(new { Message = "Đã đổi trạng thái", NewStatus = user.TrangThai });
        }
    }
}
