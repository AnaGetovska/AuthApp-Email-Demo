﻿using Dapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Email2FAuth;
using Email2FAuth.Models;

namespace Email2FAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TOTPController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly TotpService _totpService;

        public TOTPController(IConfiguration config, TotpService totpService)
        {
            _config = config;
            _totpService = totpService;
        }

        /// <summary>
        /// Setup initail TOTP secret in the DB if the generated code is valid for the secret.
        /// </summary>
        [HttpPost("setup")]
        public IActionResult SetupTOTP([FromBody] SetupTOTPModel model)
        {
            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                try
                {
                    if (_totpService.ValidateTOTP(model.Secret, model.Code))
                    {
                        var sql = $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                                  "UPDATE Users SET AuthSecret = @Secret, IsAuthEnabled = 1 WHERE Username = @Username";
                        connection.Execute(sql, new { Username = model.Username, model.Secret });
                        return Ok();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

                return BadRequest("Invalid Code for authentication. Please, try again.");
            }
        }

        /// <summary>
        /// Confirms if the passed code as parameter is valid for the User's AuthSecret in the DB.
        /// </summary>
        [HttpPost("confirm")]
        public IActionResult ConfirmTOTP([FromBody] ConfirmTOTPModel model)
        {
            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                var secret = connection.QuerySingle<string>(
                    $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                    "SELECT AuthSecret FROM Users WHERE Username = @Username",
                    new { Username = model.Username });


                if (_totpService.ValidateTOTP(secret, model.Code))
                {
                    return Ok();
                }

                return BadRequest("Invalid TOTP code.");
            }
        }

        [HttpGet("is-enabled")]
        public IActionResult CheckIfAuthEnabled([FromQuery] string username)
        {
            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                var isEnabled = connection.QuerySingle<bool>(
                    $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                    "SELECT IsAuthEnabled FROM Users WHERE Username = @Username",
                    new { username });

                if (isEnabled)
                {
                    return Ok(new { Message = "Email authentication is enabled for the user." });
                }

                return BadRequest(new { Error = "User has not enabled Email authentication." });
            }
        }
    }
}
