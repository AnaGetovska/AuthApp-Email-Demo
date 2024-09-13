using Dapper;
using Email2FAuth.Models;
using Email2FAuth.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace Email2FAuth.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly PasswordService _passwordService;
        private readonly TotpService _totpService;
        private readonly IEmailSender _emailSender;

        public AuthController(IConfiguration config, PasswordService PasswordService, TotpService totpService, IEmailSender sender)
        {
            _config = config;
            _passwordService = PasswordService;
            _totpService = totpService;
            _emailSender = sender;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Validate username and password logic here...
            var hashedPass = _passwordService.HashPassword(model.Password);
            var sql = $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                      "SELECT Email FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                var email = connection.QuerySingleOrDefault<string>(sql, new { Username = model.Username, PasswordHash = hashedPass });

                if (email != null)
                {
                    var secret = _totpService.RetrieveStoredSecterForUser(model.Username);
                    var otp = _totpService.GenerateTOTP(secret);
                    await _emailSender.SendEmailAsync(email, otp, model.Username);
                    //Console.WriteLine(otp);
                    return Ok();
                }

                return StatusCode(500);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterModel model)
        {
            var hashedPass = _passwordService.HashPassword(model.Password);
            var sql = $"USE {_config.GetRequiredSection("DB:Name").Value};" + "INSERT INTO Users (Username, PasswordHash, Email, AuthSecret) VALUES (@Username, @PasswordHash, @Email, @AuthSecret)";

            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                try
                {
                    var sqlUserExists = $"USE {_config.GetRequiredSection("DB:Name").Value};" + "SELECT * FROM Users WHERE Username = @Username";
                    var existingUser = connection.QuerySingleOrDefault(sqlUserExists, new { model.Username });

                    if (existingUser != null)
                    {
                        return BadRequest("Username already exists.");
                    }
                    var authSecret = _totpService.GenerateAuthSecret();
                    connection.Execute(sql, new { Username = model.Username, PasswordHash = hashedPass, Email = model.Email, AuthSecret = authSecret });

                    return Ok("User registered successfully.");
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> Verify(string otp, string userId)
        {
            // Retrieve the correct OTP from wherever it is stored (e.g., session, database)
            var validOtp = _totpService.RetrieveStoredSecterForUser(userId);

            if (validOtp == otp)
            {
                // OTP is valid, proceed to log the user in
                return RedirectToAction("LoginSuccess");
            }
            else
            {
                // OTP is invalid
                return BadRequest("Invalid OTP");
            }
        }
    }
}
