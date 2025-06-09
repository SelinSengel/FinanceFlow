using System.Text.RegularExpressions;
using FinanceFlow.API.Data;
using FinanceFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly FinanceFlowDbContext _context;

        public AuthController(IAuthService authService, FinanceFlowDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost("register-individual")]
        public async Task<IActionResult> RegisterIndividual([FromBody] RegisterModel dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var pwdPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$");
            if (!pwdPattern.IsMatch(dto.Password!))
                return BadRequest("Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Bu e-posta zaten kayıtlı. Şifrenizi mi unuttunuz?");

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Bu kullanıcı adı zaten alınmış.");

            var tokenResponse = await _authService.RegisterAsync(dto);

            return Ok(tokenResponse);
        }

        //[AllowAnonymous]
        //[HttpPost("register-corporate")]
        //public async Task<IActionResult> RegisterCorporate([FromBody] CorporateRegisterModel dto)
        //{
        //    if (!ModelState.IsValid)
        //        return ValidationProblem(ModelState);

        //    var pwdPattern = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$");
        //    if (!pwdPattern.IsMatch(dto.Password))
        //        return BadRequest("Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.");

        //    if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        //        return BadRequest("Bu e-posta zaten kayıtlı. Şifrenizi mi unuttunuz?");

        //    if (await _context.CorporateProfiles
        //            .AnyAsync(c => c.CompanyName == dto.CompanyName && c.TaxNumber == dto.TaxNumber))
        //        return BadRequest("Bu şirket zaten kayıtlı.");

        //    var tokenResponse = await _authService.RegisterCorporateAsync(dto);
        //    return Ok(tokenResponse);
        //}

        [AllowAnonymous]
        [HttpPost("login-individual")]
        public async Task<IActionResult> LoginIndividual([FromBody] LoginModel dto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized("Bu e-posta ile kayıtlı kullanıcı bulunamadı.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("E-posta veya şifre hatalı.");

            var tokenResponse = await _authService.LoginAsync(dto);
            return Ok(tokenResponse);
        }

        //[AllowAnonymous]
        //[HttpPost("login-corporate")]
        //public async Task<IActionResult> LoginCorporate([FromBody] LoginModel dto)
        //{
        //    var user = await _context.Users
        //        .SingleOrDefaultAsync(u => u.Email == dto.Email && u.Role == "Corporate");
        //    if (user == null)
        //        return Unauthorized("Bu e-posta ile kayıtlı kurumsal kullanıcı bulunamadı.");

        //    if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        //        return Unauthorized("E-posta veya şifre hatalı.");

        //    var tokenResponse = await _authService.LoginAsync(dto);
        //    return Ok(tokenResponse);
        //}

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (!await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Bu e-posta ile kayıtlı kullanıcı bulunamadı.");

            await _authService.ForgotPasswordAsync(dto.Email!);
            return Ok("Şifre sıfırlama talimatları e-posta adresinize gönderildi.");
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var success = await _authService.ResetPasswordAsync(dto.Token!, dto.NewPassword!);
            if (!success)
                return BadRequest("Geçersiz veya süresi dolmuş token ya da zayıf şifre.");

            return Ok("Şifre başarıyla sıfırlandı.");
        }
    }
}
