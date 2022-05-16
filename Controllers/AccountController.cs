using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService) :base(context)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(RegisterDto register)
        {
            
            AppUser user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == register.username.ToLower());

            if(user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            byte[] computecHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.password));

            for(int i = 0; i< computecHash.Length; i++ )
            {
                if(computecHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new UserDto 
            {
                Username = user.UserName,
                token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(RegisterDto register)
        {
            if(await UserExists(register.username)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();

            AppUser user = new AppUser
            {
                UserName = register.username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.password )),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }


        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}