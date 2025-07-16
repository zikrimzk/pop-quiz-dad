using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopQuizApi.Models;
using PopQuizApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PopQuizApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MyDbContext _context;

        public UsersController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<object>> login([FromBody]User user)
        {

            if(string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Username))
            {
                return BadRequest("Missing username or password");
            }

            user.encryptPassword();
            var u = _context.Users.ToList().FirstOrDefault(x=>x.Username == user.Username && x.Password == user.Password);

            if(u == null)
            {
                return Unauthorized("Invalid login");
            }

            var charlist = Enumerable.Range(0, 26).Select(x => (char)('A' + x)).ToList();
            var numberlist = Enumerable.Range(0, 10).Select(x => x.ToString()).ToList();

            Random rand = new Random();
            var list = string.Join("", charlist) + string.Join("", charlist.Select(x => char.ToLower(x))) + string.Join("", numberlist);
            var remember_token = string.Join("", Enumerable.Range(0, 20).Select(x => list[rand.Next(0, list.Count() - 1)])) + user.Id + DateTime.Now.ToString("yyyyhhddmmssMM");

            u.RememberToken = remember_token;
            u.TokenExpiredAt = DateTime.Now.AddHours(12);

            _context.SaveChanges();

            return new { token = u.RememberToken};
        }

        // GET: api/Users
 
        [HttpGet("validate")]
        public  async Task<ActionResult> GetUser([FromHeader] string token)
        {
            if(!await AuthService.validateUser(HttpContext))
            {
                return Unauthorized();
            }

            var user = HttpContext.Items["User"] as User;
            if (user == null)
            {
                return NotFound();
            }



           
            return Ok(new{ user.Username, user.Email,user.TokenExpiredAt});



        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        
        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754


//        {
//  "username": "mx",
//  "password": "Abc@123",
//  "email": "xx@xx.xx"
 
//}
        [HttpPost("register")]
        public async Task<ActionResult<User>> PostUser(User user)
        {

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password)) { 
                return BadRequest();
            }

            var regex = @"[A-Za-z][\w\.\-]*@[\w\.\-]+\.[A-Za-z]{2,}";

            if(!Regex.IsMatch(user.Email, regex))
            {
                return BadRequest("Invalid email format");
            }

            var pass_regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*^[a-zA-Z0-9]).{6,}$"
;
            if(!Regex.IsMatch(user.Password, pass_regex))
            {
                return BadRequest("Password should contains at least one Uppercase, one lowercase, one special character and 6 characters long");
            }
            
            if (_context.Users.Any(x=>x.Username == user.Username))
            {
                return BadRequest("The username has been used");
            }

            if (_context.Users.Any(x => x.Email == user.Email))
            {
                return BadRequest("This email has been used");
            }


            user.encryptPassword();

            user.CreatedAt = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { user.Email, user.CreatedAt, user.Username});
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
