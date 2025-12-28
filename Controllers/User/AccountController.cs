using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PTPMHDV.Models;
namespace PTPMHDV.Controllers
{
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();

        // 2.3.7.1 Xem thông tin tài khoản
        // GET: api/account/profile/ND001
        [HttpGet]
        [Route("profile/{maND}")]
        public IHttpActionResult GetProfile(string maND)
        {
            var user = db.NGUOIDUNGs
                .Where(x => x.MaND == maND)
                .Select(x => new
                {
                    x.MaND,
                    x.TenND,
                    x.Email,
                    x.SDT
                })
                .FirstOrDefault();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // 2.3.7.2 Cập nhật thông tin tài khoản
        // PUT: api/account/update
        [HttpPut]
        [Route("update")]
        public IHttpActionResult UpdateProfile(NGUOIDUNG model)
        {
            var user = db.NGUOIDUNGs.FirstOrDefault(x => x.MaND == model.MaND);
            if (user == null)
                return NotFound();

            user.TenND = model.TenND;
            user.Email = model.Email;
            user.SDT = model.SDT;

            db.SaveChanges();
            return Ok("Cập nhật thông tin tài khoản thành công");
        }

        // 2.3.7.3 Đổi mật khẩu
        // PUT: api/account/change-password
        [HttpPut]
        [Route("change-password")]
        public IHttpActionResult ChangePassword(string maND, string matKhauCu, string matKhauMoi)
        {
            var user = db.NGUOIDUNGs
                .FirstOrDefault(x => x.MaND == maND && x.MatKhau == matKhauCu);

            if (user == null)
                return BadRequest("Mật khẩu cũ không đúng");

            user.MatKhau = matKhauMoi;
            db.SaveChanges();

            return Ok("Đổi mật khẩu thành công");
        }
    }
}
