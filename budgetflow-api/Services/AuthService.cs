using BudgetFlow.API.Data;
using BudgetFlow.API.DTOs.Auth;
using BudgetFlow.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BudgetFlow.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public AuthService(UserManager<AppUser> userManager, AppDbContext context, IConfiguration config)
        {
            _userManager = userManager;
            _context = context;
            _config = config;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new Exception("Invalid Email or Password.");
            var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!validPassword)
                throw new Exception("Invalid Email or Password.");
            return await GenerateAuthResponse(user);
        }

        Task<AuthResponseDto> IAuthService.RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new Exception("Email already in use.");
            }
            var user = new AppUser
            {
                Email = dto.Email,
                FullName = dto.FullName,
                UserName = dto.Email
            };
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            return await GenerateAuthResponse(user);
        }


        Task IAuthService.RevokeTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }
        private (string Token, DateTime ExpiresAt) GenerateJwt(AppUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!));
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id),
                new Claim(JwtRegisteredClaimNames.Email,user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim("fullName",user.FullName),
            };
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audiance"],
                claims: claims,
                signingCredentials: creds,
                expires: expiresAt
                );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
        private async Task<string> CreateRefreshToken(AppUser user)
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes);
            var refreshToken = new RefreshToken
            {
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!))
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return token;
        }
        private async Task<AuthResponseDto> GenerateAuthResponse(AppUser user)
        {
            var (token, expiresAt) = GenerateJwt(user);
            var refreshToken = await CreateRefreshToken(user);

            return new AuthResponseDto
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Email = user.Email!
            };
        }
    }
}