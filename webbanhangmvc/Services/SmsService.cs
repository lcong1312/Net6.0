using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace webbanhangmvc.Services
{
    public class SmsService
    {
        private readonly TwilioSettings _twilioSettings;

        public SmsService(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
        }

        public async Task SendSmsAsync(string toPhoneNumber, string message)
        {
            // Chuẩn hóa theo định dạng E.164
            toPhoneNumber = NormalizePhoneNumber(toPhoneNumber, "+84");

            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);

            var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
            {
                From = new PhoneNumber(_twilioSettings.FromNumber),
                Body = message
            };

            await MessageResource.CreateAsync(messageOptions);
        }

        private string NormalizePhoneNumber(string phoneNumber, string countryCode)
        {
            if (phoneNumber.StartsWith("0"))
            {
                phoneNumber = phoneNumber.Substring(1);
            }
            return $"{countryCode}{phoneNumber}";
        }
    }
public class TwilioSettings
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string FromNumber { get; set; }
    }
}
