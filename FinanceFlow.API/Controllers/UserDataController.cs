using FinanceFlow.API.Data;
using FinanceFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace FinanceFlow.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserDataController : ControllerBase
    {
        private readonly FinanceFlowDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UserDataController(FinanceFlowDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var dto = new UserModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role
            };
            return Ok(dto);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UserModel model)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("Token geçersiz");

                var userId = int.Parse(userIdClaim);
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return NotFound("Kullanıcı bulunamadı");

                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.FullName))
                    return BadRequest("Kullanıcı adı ve isim boş olamaz.");

                var usernameExists = await _context.Users
                    .AnyAsync(u => u.Username == model.Username && u.Id != userId);
                if (usernameExists)
                    return BadRequest("Bu kullanıcı adı zaten kullanımda.");

                user.Username = model.Username;
                user.FullName = model.FullName;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException dbEx)
            {
                var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, $"Veritabanı hatası: {inner}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Beklenmeyen hata: {ex.Message}");
            }
        }


        [HttpPost("me/avatar")]
        [RequestSizeLimit(5 * 1024 * 1024)]  
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            if (avatar == null || avatar.Length == 0)
                return BadRequest("Lütfen bir dosya seçin.");

            var userId = int.Parse(userIdClaim);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Kaydedilecek klasörü hazırla
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsDir);

            // Dosya adı: avatar-{userId}{.jpg/.png}
            var ext = Path.GetExtension(avatar.FileName);
            var fileName = $"avatar-{userId}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Fiziksel dosyayı kaydet
            using var stream = System.IO.File.Create(filePath);
            await avatar.CopyToAsync(stream);

            // DB’de URL’yi güncelle
            user.AvatarUrl = $"/uploads/{fileName}";
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Yanıt olarak yeni URL’yi dön
            return Ok(new { user.AvatarUrl });
        }
    }
}
