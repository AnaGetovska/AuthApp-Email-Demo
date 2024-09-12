using Dapper;
using OtpNet;
using System.Data.SqlClient;

namespace Email2FAuth
{
    public class TotpService
    {
        private readonly IConfiguration _config;
        public TotpService(IConfiguration config)
        {
            _config = config;
        }
        public string GenerateAuthSecret()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }

        public string GenerateTOTP(string secret)
        {
            var secretBytes = Base32Encoding.ToBytes(secret);

            // Create a Totp instance using the secret, valid for specified time window in seconds.
            var totp = new Totp(secretBytes, step: 300);

            string totpCode = totp.ComputeTotp();

            return totpCode;
        }

        public bool ValidateTOTP(string secret, string code)
        {
            var totp = new Totp(Base32Encoding.ToBytes(secret), step: 300);
            return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }

        public string RetrieveStoredSecterForUser(string username)
        {
            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                return connection.QuerySingle<string>(
                $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                "SELECT AuthSecret FROM Users WHERE Username = @Username",
                new { Username = username });
            }
        }
    }
}
