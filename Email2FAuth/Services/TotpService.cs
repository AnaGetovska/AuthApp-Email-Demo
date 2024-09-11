using OtpNet;
using QRCoder;
using System.Drawing;

namespace Email2FAuth
{
    public class TotpService
    {
        public string GenerateAuthSecret()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        public string GenerateTOTP(string secret)
        {
            // Decode the base32 secret key
            var secretBytes = Base32Encoding.ToBytes(secret);

            // Create a Totp instance using the secret key
            var totp = new Totp(secretBytes, step: 300);

            // Generate the TOTP code for the current time window
            string totpCode = totp.ComputeTotp();

            return totpCode;
        }

        public bool ValidateTOTP(string secret, string code)
        {
            var totp = new Totp(Base32Encoding.ToBytes(secret), step: 300);
            return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
    }
}
