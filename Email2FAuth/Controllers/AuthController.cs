using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Email2FAuth.Models;
using Email2FAuth;
using System.Reflection;
using Email2FAuth.Services;

namespace TotpDemo.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _config;
        private readonly PasswordService _passwordService;
        private readonly TotpService _totpService;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IConfiguration config, PasswordService PasswordService, TotpService totpService, IEmailSender sender, IHttpContextAccessor context)
        {
            _config = config;
            _connection = new SqlConnection(_config.GetConnectionString("Default"));

            _passwordService = PasswordService;
            _totpService = totpService;
            _emailSender = sender;
            _httpContextAccessor = context;
        }

        public record SendUserData
        {
            public string Id { get; set; }
            public string Email { get; set; }
        }

        public string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var host = request.Host.ToUriComponent();
            var pathBase = request.PathBase.ToUriComponent();

            return $"{request.Scheme}://{host}{pathBase}";
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            // Validate username and password logic here...
            var hashedPass = _passwordService.HashPassword(model.Password);
            var sql = $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                      "SELECT Id, Email FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
            var user = _connection.QuerySingleOrDefault<SendUserData>(sql, new { Username = model.Username, PasswordHash = hashedPass });

            if (user?.Id != null)
            {
                //var secret = RetrieveStoredSecterForUser(model.Username);
                //var otp = _totpService.GenerateTOTP(secret);
                //_emailSender.SendEmailAsync(user.Email, otp, user.Id, GetBaseUrl());
                return Ok();
            }

            return StatusCode(500);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterModel model)
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
        public IActionResult Verify(string otp, string userId)
        {
            // Retrieve the correct OTP from wherever it is stored (e.g., session, database)
            var validOtp = RetrieveStoredSecterForUser(userId);

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

        private string RetrieveStoredSecterForUser(string username)
        {
            return _connection.QuerySingle<string>(
                $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                "SELECT AuthSecret FROM Users WHERE Username = @Username",
                new { Username = username });
        }
    }
}
