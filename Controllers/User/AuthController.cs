using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using PTPMHDV.Models;

namespace PTPMHDV.Controllers
{
    public class AuthController : ApiController
    {
        QuanLyBanHangEntities1 db = new QuanLyBanHangEntities1();
        //HÀM MÃ HÓA MD5 
        private string MD5Encode(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }


        // 1. Đăng ký
        [HttpPost]
        [Route("api/auth/register")]
        public string Register(NGUOIDUNG u)
        {
            if (u == null) return "Dữ liệu sai";

            if (db.NGUOIDUNGs.Any(x => x.TaiKhoan == u.TaiKhoan))
                return "Tài khoản đã tồn tại";

            u.MaND = "ND" + (db.NGUOIDUNGs.Count() + 1);
            u.MatKhau = MD5Encode(u.MatKhau);
            u.VaiTro = "User";

            db.NGUOIDUNGs.Add(u);
            db.SaveChanges();

            return "Đăng ký thành công";
        }

        // 2. Kiểm tra username
        [HttpGet]
        [Route("api/auth/check-username/{tk}")]
        public bool CheckUsername(string tk)
        {
            return db.NGUOIDUNGs.Any(x => x.TaiKhoan == tk);
        }

        // 3. Kiểm tra email
        [HttpGet]
        [Route("api/auth/check-email/{email}")]
        public bool CheckEmail(string email)
        {
            return db.NGUOIDUNGs.Any(x => x.Email == email);
        }


        // 4. Kiểm tra mật khẩu mạnh
        [HttpGet]
        [Route("api/auth/validate-password/{pw}")]
        public bool ValidatePassword(string pw)
        {
            return pw.Length >= 6;
        }


        // 5. Đăng nhập
        [HttpGet]
        [Route("api/auth/login")]
        public string Login(string username, string password)
        {
            var user = db.NGUOIDUNGs
                         .FirstOrDefault(x => x.TaiKhoan == username
                                           && x.MatKhau == password);

            if (user == null)
                return "Sai tài khoản hoặc mật khẩu!";

            return "Đăng nhập thành công! Xin chào: " + user.TenND
                   + " (Vai trò: " + user.VaiTro + ")";
        }


        // 6. Quên mật khẩu
        [HttpGet]
        [Route("api/auth/forgot/{email}")]
        public string Forgot(string email)
        {
            bool exists = db.NGUOIDUNGs.Any(x => x.Email == email);
            return exists ? "Email tồn tại" : "Không tìm thấy email";
        }


        // 7. Reset mật khẩu
        [HttpGet]
        [Route("api/auth/reset/{email}/{newpw}")]
        public string Reset(string email, string newpw)
        {
            var u = db.NGUOIDUNGs.FirstOrDefault(x => x.Email == email);
            if (u == null) return "Không tìm thấy user";
            u.MatKhau = MD5Encode(newpw);
            db.SaveChanges();
            return "Đặt lại mật khẩu thành công";
        }


        // 8. Kiểm tra mật khẩu MD5
        [HttpGet]
        [Route("api/auth/md5/{pw}")]
        public string EncodePassword(string pw)
        {
            return MD5Encode(pw);
        }
    }

    public class LoginModel
    {
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
    }
}
