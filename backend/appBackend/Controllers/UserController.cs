// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using Microsoft.IdentityModel.Tokens;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using appBackend.Services;
// namespace appBackend.Models;

// [ApiController]
// [Route("api/user")]
// public class UserController : ControllerBase
// {
//     private readonly string _key;
//     private readonly UserService _userService;

//     public UserController(IConfiguration configuration, UserService userService)
//     {
//         _userService = userService;
//     }

//     [HttpPost("id")]
//     public async Task<IActionResult> GetUser([FromBody] string request)
//     {
//         var validated = await _userService.ValidateUserAsync(request.Username, request.Password);
//         if (validated) // Replace with real authentication
//         {
//             var tokenHandler = new JwtSecurityTokenHandler();
//             var key = Encoding.UTF8.GetBytes(_key);
//             var tokenDescriptor = new SecurityTokenDescriptor
//             {
//                 Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, request.Username) }),
//                 Expires = DateTime.UtcNow.AddHours(1),
//                 SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//             };

//             var token = tokenHandler.CreateToken(tokenDescriptor);
//             return Ok(new { token = tokenHandler.WriteToken(token) });
//         }
//         return Unauthorized();
//     }
// }
