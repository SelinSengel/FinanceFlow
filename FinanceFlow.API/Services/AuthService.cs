using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FinanceFlow.API.Data;
using FinanceFlow.API.Entities;
using FinanceFlow.Shared.Models;

namespace FinanceFlow.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly FinanceFlowDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IConfiguration config,
            FinanceFlowDbContext context,
            ILogger<AuthService> logger)
        {
            _config = config;
            _context = context;
            _logger = logger;
        }

        public async Task<TokenResponseModel?> RegisterAsync(RegisterModel dto)
        {
            if (!IsPasswordStrong(dto.Password!))
                return null;

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email) ||
                await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return null;

            // DTO → Entity
            var userEntity = new UserEntity
            {
                Username = dto.Username!,
                Email = dto.Email!,
                FullName = dto.FullName!,
                AvatarUrl = dto.AvatarUrl!,
                Role = dto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password!),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync();

            return CreateTokenResponse(userEntity);
        }

        public async Task<TokenResponseModel?> RegisterCorporateAsync(CorporateRegisterModel dto)
        {
            if (!IsPasswordStrong(dto.Password))
                return null;

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email) ||
                await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return null;

            if (await _context.CorporateProfiles
                    .AnyAsync(c => c.CompanyName == dto.CompanyName && c.TaxNumber == dto.TaxNumber))
                return null;

            var companyEntity = new CorporateProfileEntity
            {
                CompanyName = dto.CompanyName,
                TaxNumber = dto.TaxNumber,
                ContactEmail = dto.Email,
                Sector = dto.Sector
            };
            _context.CorporateProfiles.Add(companyEntity);
            await _context.SaveChangesAsync();

            var userEntity = new UserEntity
            {
                Username = dto.Username,
                Email = dto.Email,
                Role = "Corporate",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                CorporateProfileId = companyEntity.Id
            };
            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync();

            return CreateTokenResponse(userEntity);
        }

        public async Task<TokenResponseModel?> LoginAsync(LoginModel dto)
        {
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (userEntity == null ||
                !BCrypt.Net.BCrypt.Verify(dto.Password!, userEntity.PasswordHash))
                return null;

            return CreateTokenResponse(userEntity);
        }

        private TokenResponseModel CreateTokenResponse(UserEntity userEntity)
        {
            var jwt = CreateJwt(userEntity);
            return new TokenResponseModel
            {
                Token = jwt,
                Role = userEntity.Role,
                Username = userEntity.Username,
                FullName = userEntity.FullName,
                Email = userEntity.Email
            };
        }

        private string CreateJwt(UserEntity userEntity)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, userEntity.Id.ToString()),
        new Claim("userId", userEntity.Id.ToString()),  
        // FullName’i Name claim’ine koyuyoruz
        new Claim(ClaimTypes.Name,           userEntity.FullName),
        // Username’i custom bir claim’e
        new Claim("username",                userEntity.Username),
        new Claim(ClaimTypes.Email,          userEntity.Email),
        new Claim(ClaimTypes.Role,           userEntity.Role)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private bool IsPasswordStrong(string password)
        {
            return password.Length >= 8
                && password.Any(char.IsUpper)
                && password.Any(char.IsLower)
                && password.Any(char.IsDigit)
                && password.Any(ch => !char.IsLetterOrDigit(ch));
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (userEntity == null) return false;

            _context.PasswordResetTokens.RemoveRange(
                _context.PasswordResetTokens.Where(t => t.UserId == userEntity.Id));

            var token = Guid.NewGuid().ToString("N");
            _context.PasswordResetTokens.Add(new PasswordResetTokenEntity
            {
                UserId = userEntity.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var prt = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (prt == null || prt.ExpiresAt < DateTime.UtcNow)
                return false;

            if (!IsPasswordStrong(newPassword))
                return false;

            prt.User!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.PasswordResetTokens.Remove(prt);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
