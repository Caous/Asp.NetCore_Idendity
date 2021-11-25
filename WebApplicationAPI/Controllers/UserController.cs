using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.Domain;
using WebApplicationAPI.Dto;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplicationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;

        public UserController(IConfiguration config, UserManager<User> userManager, SignInManager<User> signInManager, IMapper mapper)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }
        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserController>
        [HttpPost]
        public void Post(UserDto value)
        {
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userLoginDto.UserName);

                var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginDto.Password, false);

                if (result.Succeeded)
                {
                    var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == user.UserName.ToUpper());
                    var userToReturn = _mapper.Map<UserDto>(appUser);
                    return Ok(new { token = GenerateJWT(appUser).Result, user = appUser });

                }
                return Unauthorized();
            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, $"ERROR { ex.Message}");
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            // Podemos fazer validação do data annotation assim
            try
            {


                var user = await _userManager.FindByNameAsync(userDto.UserName);

                if (user == null)
                {
                    user = new User()
                    {
                        UserName = userDto.UserName,
                        PasswordHash = userDto.Password,
                        Email = userDto.UserName


                    };

                    var result = await _userManager.CreateAsync(
                        user, userDto.Password);

                    if (result.Succeeded)
                    {
                        var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == user.UserName.ToUpper());

                        var token = GenerateJWT(appUser).Result;

                        //var confirmationEmail = Url.Action("ConfirmEmailAddress", "Home",
                        //    new { token = token, email = user.Email }, Request.Scheme);

                        //System.IO.File.WriteAllText("confirmedEmail.txt", confirmationEmail);
                        return Ok(token);
                    }
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, $"ERROR { ex.Message}");
            }

        }

        private async Task<string> GenerateJWT(User user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var roles = await _userManager.GetRolesAsync(user);

            foreach (var item in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, item));

            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDesciption = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDesciption);

            return tokenHandler.WriteToken(token);



        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
