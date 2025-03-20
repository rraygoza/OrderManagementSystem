using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            Console.WriteLine($"JWT Key: {_configuration["Jwt:Key"]}");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequestDto model)
        {
            if (IsValidUser(model.Username, model.Password))
            {
                string userId = GetUserId(model.Username);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(JwtRegisteredClaimNames.Iss, _configuration["Jwt:Issuer"]),
                    new Claim(JwtRegisteredClaimNames.Aud, "OrderService"),
                    new Claim(JwtRegisteredClaimNames.Aud, "CustomerService"),
                    new Claim(JwtRegisteredClaimNames.Aud, "ProductService")
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationMinutes"] ?? "30")),
                    signingCredentials: creds
                );

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Unauthorized();
        }

        private bool IsValidUser(string username, string password)
        {
            if (username == "testuser" && password == "testpassword")
            {
                return true;
            }
            return false;
        }

        private string GetUserId(string username)
        {
            if (username == "testuser")
            {
                return "1";
            }

            return null;
        }
    }

    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
