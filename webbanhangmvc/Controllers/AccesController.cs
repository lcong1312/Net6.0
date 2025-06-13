using Microsoft.AspNetCore.Mvc;
using webbanhangmvc.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using webbanhangmvc.Services;

namespace webbanhangmvc.Controllers
{
    public class AccesController : Controller
    {
        private readonly QlbanVaLiContext db;
        private readonly EmailService _emailService;
        private readonly SmsService _smsService;

        public AccesController(QlbanVaLiContext context,EmailService emailService,SmsService smsService)
        {
            db = context;
            _emailService = emailService;
            _smsService = smsService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(TUserKh user)
        {
            if (HttpContext.Session.GetString("UserName") != null)
            {
                // Người dùng đã đăng nhập, chuyển hướng đến trang chính
                return RedirectToAction("Index", "Home");
            }

            var u = db.TUserKhs.SingleOrDefault(x => x.UsernameKh == user.UsernameKh && x.Password == user.Password);

            if (u != null)
            {
                HttpContext.Session.SetString("UserName", u.UsernameKh);
                return RedirectToAction("Index", "Home");
            }

            // Đăng nhập không thành công, hiển thị lại trang đăng nhập
            return View();
        }



        //    [HttpPost("send-mail")]
        //    public async Task<IActionResult> SendMail([FromBody] SendOtpRequest request)
        //    {
        //        var otp = new Random().Next(10000, 99999).ToString();
        //        if (!string.IsNullOrEmpty(request.Email))
        //        {
        //            var subject = "Mã Xác Minh";
        //            var message = $"Mã xác minh của bạn là: {otp}";
        //            await _emailService.SendEmailAsync(request.Email, subject, message);
        //        }
        //        return Ok(new { Message = "Gửi Thành Công Mã OTP." });
        //    }

        //[HttpPost("send-otp")]
        //    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        //    {
        //        var otp = new Random().Next(10000, 99999).ToString();
        //        if (!string.IsNullOrEmpty(request.PhoneNumber))
        //        {
        //            var subject = "Mã Xác Minh";
        //            var message = $"Mã xác minh của bạn là: {otp}";
        //            await _smsService.SendSmsAsync(request.PhoneNumber, message);
        //        }
        //        return Ok(new { Message = "Gửi Thành Công Mã OTP." });
        //    }

        [HttpPost]
        public IActionResult CheckUser(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập email hoặc số điện thoại.";
                return View("RecoverAccount");
            }

            var user = db.TUserKhs.SingleOrDefault(p => p.SoDienThoai1 == identifier || p.Email == identifier);

            if (user != null)
            {
                 HttpContext.Session.SetString("Identifier", identifier);
                return RedirectToAction("SendOtp", new { identifier = identifier });
            }
            else
            {
                ViewBag.ErrorMessage = "Không tìm thấy người dùng.";
                return View("RecoverAccount");
            }
        }

        [HttpGet]
        public IActionResult SendOtp()
        {
            string identifier = HttpContext.Session.GetString("Identifier");
            if (string.IsNullOrEmpty(identifier))
            {
                return RedirectToAction("RecoverAccount");
            }
            ViewBag.Identifier = identifier;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendOtp(string identifier)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("Otp", otp);
            

            if (!string.IsNullOrEmpty(identifier))
            {
                if (identifier.Contains("@"))
                {
                    var subject = "Mã Xác Minh";
                    var message = $"Mã xác minh của bạn là: {otp}";
                    await _emailService.SendEmailAsync(identifier, subject, message);
                }
                else
                {
                    var message = $"Mã xác minh của bạn là: {otp}";
                    await _smsService.SendSmsAsync(identifier, message);
                }

                return RedirectToAction("VerifyOtp");
            }

            return View();
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string otp)
        {
            var sessionOtp = HttpContext.Session.GetString("Otp");

            if (otp == sessionOtp)
            {
                HttpContext.Session.SetString("IsOtpVerified", "true");
                return RedirectToAction("ResetPassword");
            }

            ViewBag.ErrorMessage = "Mã xác minh không đúng.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string newPassword)
        {
            var isOtpVerified = HttpContext.Session.GetString("IsOtpVerified");
            if (isOtpVerified != "true")
            {
                ViewBag.ErrorMessage = "Bạn phải xác minh OTP trước khi đổi mật khẩu.";
                return View("VerifyOtp");
            }
            var identifier = HttpContext.Session.GetString("Identifier");
            var user = db.TUserKhs.SingleOrDefault(p => p.SoDienThoai1 == identifier || p.Email == identifier);

            if (user != null)
            {
                user.Password = newPassword; 
                db.SaveChanges();

                HttpContext.Session.Remove("IsOtpVerified");
                HttpContext.Session.Remove("Otp");
                HttpContext.Session.Remove("Identifier");

                return RedirectToAction("Login", "Acces");
            }

            ViewBag.ErrorMessage = "Đã xảy ra lỗi khi đặt lại mật khẩu.";
            return View("ResetPassword");
        }

        [HttpGet]
        public IActionResult RecoverAccount()
        {
            return View();
        }

        //public IActionResult Logout()
        //{
        //    HttpContext.Session.Clear();
        //    return RedirectToAction("Login", "Acces");
        //}

        [HttpGet] // Đảm bảo đây là yêu cầu GET
        public IActionResult Register()
        {
            // Nếu đã đăng nhập, chuyển hướng đến trang chính
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(TUserKh user)
        {
            // Kiểm tra xem đã đăng nhập hay chưa
            if (HttpContext.Session.GetString("UserName") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên người dùng đã tồn tại chưa
                if (db.TUserKhs.Any(u => u.UsernameKh == user.UsernameKh))
                {
                    ModelState.AddModelError("Username", "Tên người dùng đã tồn tại.");
                    return View(user);
                }

                db.TUserKhs.Add(user);
                db.SaveChanges();
                return RedirectToAction("Login", "Acces");
            }

            // Đăng ký không thành công, hiển thị lại trang đăng ký
            return View(user);
        }
    }
}
public class CheckUserRequest
{
    public string Identifier { get; set; }
}

public class OtpRequest
{
    public string Identifier { get; set; }
}