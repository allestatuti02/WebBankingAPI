using WebBankingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebBankingAPI.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class LoginController : Controller
    {
        [HttpPost("Login")]
        public ActionResult Login([FromBody] User credentials)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                User candidate = model.Users.FirstOrDefault(q => q.Username == credentials.Username && q.Password == credentials.Password);
                if (candidate == null)
                    return Problem("Username e/o password errati");

                var tokenHundler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    SigningCredentials = new SigningCredentials(SecurityKeyGenerator.GetSecurityKey(), SecurityAlgorithms.HmacSha256Signature),
                    Expires = DateTime.UtcNow.AddDays(1),
                    Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                            new Claim("id", candidate.Username),
                            new Claim("state", candidate.IsBanker.ToString())
                        }
                    )
                };
                SecurityToken token = tokenHundler.CreateToken(tokenDescriptor);

                candidate.LastLogin = DateTime.Now;
                model.SaveChanges();

                return Ok(tokenHundler.WriteToken(token));
            }
        }

        // 7 - Attivare il logout (solamente se un utente è Loggato) con Authorize
        [Authorize]
        [HttpPost("Logout")]
        public ActionResult Logout()
        {
            string username = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
            using (WebBankingContext model = new WebBankingContext())
            {
                //var userId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "id").Value;
                User candidate = model.Users.FirstOrDefault(q => q.Username == username);

                if (candidate != null)
                {
                    candidate.LastLogout = DateTime.Now;
                    model.SaveChanges();

                    return Ok("Logout effettuato correttamente");
                }
                else
                    return NotFound();

                
            }
        }
    }
}
