using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SecuredAuthApp.DTOs;
using SecuredAuthApp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecuredAuthApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        //api/account/register
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  
            }
            var user = new AppUser()
            {
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                UserName = registerDto.Email
            };
            var result= await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            if(registerDto.Roles is null)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                foreach (var role in registerDto.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            return Ok(new AuthRespondDtos
            {
                IsSuccess = true,
                Message = "Account Created Successfully !!"
            });
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthRespondDtos>> Login(LoginDto loginDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);  
            }
            var user= await _userManager.FindByEmailAsync(loginDto.Email);
            if (user is null)
            {
                return Unauthorized(new AuthRespondDtos { IsSuccess = false , Message="User Not Found with this Email"});

            }
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                return Unauthorized(new AuthRespondDtos { IsSuccess = false, Message = "Invalid Password" });
            }

            var token = GenerateToken(user);
            return Ok(new AuthRespondDtos
            {
                IsSuccess = true,
                Token = token,
                Message = "Login Success"
            });
        }
        //Generate JWT Token
        private string GenerateToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII
                .GetBytes(_configuration.GetSection("JWTSetting").GetSection("securityKey").Value!);

            var roles = _userManager.GetRolesAsync(user).Result;

            var claims = new List<Claim>
            {
                    new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                    new(JwtRegisteredClaimNames.Name, user.FullName ?? ""),
                    new(JwtRegisteredClaimNames.NameId, user.Id ?? ""),
                    new(JwtRegisteredClaimNames.Aud,
                    _configuration.GetSection("JWTSetting").GetSection("ValidAudience").Value!),
                    new(JwtRegisteredClaimNames.Iss, _configuration.GetSection("JWTSetting").GetSection("ValidIssuer").Value!)
                };
            //add roles in claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            };


            var token= tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        //api/account/detail
        [Authorize]
        [HttpGet("detail")]
        //implement get detail of current loggedin user
        public async Task<ActionResult<UserDetailDto>> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user= await _userManager.FindByIdAsync(currentUserId);
            if (user is null)
            {
                return NotFound(new AuthRespondDtos
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }
            return Ok(new UserDetailDto
            {
                Id=user.Id,
                Email=user.Email,
                FullName=user.FullName,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray(),
                PhoneNumber=user.PhoneNumber,
                PhoneNumberConfirmed=user.PhoneNumberConfirmed,
                AccessFailedCount=user.AccessFailedCount,
            });
        }   
    }
}
