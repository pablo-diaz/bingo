using System;
using System.Text;
using System.Security.Claims;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.IdentityModel.Tokens;

using Microsoft.Extensions.Configuration;

namespace WebUI.Infrastructure
{
    public class BingoSecurity
    {
        private readonly IConfiguration _configuration;
        private const string _jwtIssuer = "localhost";
        private const string _jwtAudience = "localhost";
        private readonly TimeSpan _playerTokenDuration = TimeSpan.FromHours(4);
        public static readonly string CommonUserPasswd = "1234";

        public BingoSecurity(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public string CreateJWTTokenForPlayer(string forGameName)
        {
            var playerClaims = BuildPlayerClaims(forGameName);
            var credentials = BuildSigningCredentials();
            var jwtHelper = new JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;

            var token = jwtHelper.CreateJwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                subject: playerClaims,
                notBefore: now,
                expires: now.Add(this._playerTokenDuration),
                issuedAt: now, 
                signingCredentials: credentials);

            return jwtHelper.WriteToken(token);
        }

        private ClaimsIdentity BuildPlayerClaims(string forGameName) =>
            new ClaimsIdentity(new List<Claim> {
                new Claim("GameName", forGameName)
            });

        private SigningCredentials BuildSigningCredentials()
        {
            var signingKey = this._configuration["Bingo.Security:JWTSigningKey"];
            var bytes = Encoding.UTF8.GetBytes(signingKey);
            var key = new SymmetricSecurityKey(bytes);
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }
    }
}
