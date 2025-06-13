using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webbanhangmvc.Services;

namespace webbanhangmvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendMailController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly SmsService _smsService;
        public SendMailController(EmailService emailService, SmsService smsService)
        {
            _emailService = emailService;
            _smsService = smsService;
        }


        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            // Tạo mã OTP ngẫu nhiên
            var otp = new Random().Next(10000, 99999).ToString();
            if (!string.IsNullOrEmpty(request.Email))
            {
                var subject = "Mã Xác Minh";
                var message = $"Mã xác minh của bạn là: {otp}";
                await _emailService.SendEmailAsync(request.Email, subject, message);
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var message = $"Mã xác minh của bạn là: {otp}";
                await _smsService.SendSmsAsync(request.PhoneNumber, message);
            }
            return Ok(new { Message = "Gửi Thành Công Mã OTP." });
        }

    }
    public class SendOtpRequest
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

}
