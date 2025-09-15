// Infrastructure/Login/JwtService.cs
using AtlasSasS.Domain.Abstractions.Login;
using AtlasSasS.Infrastructure.Login;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AtlasSasS.Infrastructure.Login
{
	public sealed class JwtService : IJwtService
	{
		private readonly JwtOptions _opt;
		private readonly SymmetricSecurityKey _key;
		private readonly SigningCredentials _cred;
		private readonly TokenValidationParameters _refreshValidate;
		private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Guid> _refreshStore = new();

		public JwtService(IOptions<JwtOptions> opt)
		{
			_opt = opt.Value;
			_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.HmacSecret));
			_cred = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

			_refreshValidate = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = _opt.Issuer,
				ValidateAudience = true,
				ValidAudience = _opt.Audience,
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = _key,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.FromSeconds(30)
			};
		}

		public (string accessToken, string refreshToken, DateTimeOffset accessExp, DateTimeOffset refreshExp)
			IssueTokenPair(Guid userId, IEnumerable<string> rolesClaims)
		{
			var now = DateTimeOffset.UtcNow;

			var accessClaims = new List<Claim> {
			new(JwtRegisteredClaimNames.Sub, userId.ToString()),
			new(JwtRegisteredClaimNames.Iss, _opt.Issuer),
			new(JwtRegisteredClaimNames.Aud, _opt.Audience),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
			new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
		};
			accessClaims.AddRange(rolesClaims.Select(r => new Claim("roles", r)));

			var access = new JwtSecurityToken(
				issuer: _opt.Issuer, audience: _opt.Audience,
				claims: accessClaims, notBefore: now.UtcDateTime,
				expires: now.AddMinutes(_opt.AccessMinutes).UtcDateTime,
				signingCredentials: _cred);

			var accessStr = new JwtSecurityTokenHandler().WriteToken(access);

			var refresh = new JwtSecurityToken(
				issuer: _opt.Issuer, audience: _opt.Audience,
				claims: [
				new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
				new Claim("typ","refresh")
				],
				notBefore: now.UtcDateTime,
				expires: now.AddDays(_opt.RefreshDays).UtcDateTime,
				signingCredentials: _cred);

			var refreshStr = new JwtSecurityTokenHandler().WriteToken(refresh);

			// 화이트리스트 등록
			var jti = new JwtSecurityTokenHandler().ReadJwtToken(refreshStr).Id;
			_refreshStore[jti] = userId;

			return (accessStr, refreshStr, access.ValidTo, refresh.ValidTo);
		}

		public Task<(bool ok, Guid userId)> ValidateRefreshAsync(string refreshToken)
		{
			var h = new JwtSecurityTokenHandler();
			try
			{
				var p = h.ValidateToken(refreshToken, _refreshValidate, out _);
				if (p.FindFirst("typ")?.Value != "refresh")
					return Task.FromResult((false, Guid.Empty));

				var jti = p.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
				var sub = p.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? p.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				
				if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(sub))
					return Task.FromResult((false, Guid.Empty));

				if (!_refreshStore.TryGetValue(jti, out var userId))
					return Task.FromResult((false, Guid.Empty));

				_refreshStore.TryRemove(jti, out _);

				return Task.FromResult((Guid.TryParse(sub, out var parsed) && parsed == userId, userId));
			}
			catch { return Task.FromResult((false, Guid.Empty)); }
		}

		public Task RevokeRefreshAsync(string refreshToken)
		{
			var h = new JwtSecurityTokenHandler();
			try
			{
				var jti = h.ReadJwtToken(refreshToken).Id;
				if (!string.IsNullOrEmpty(jti))
					_refreshStore.TryRemove(jti, out _);
			}
			catch { /* ignore */ }
			return Task.CompletedTask;
		}
	}

}
