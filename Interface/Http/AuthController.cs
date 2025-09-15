using AtlasSasS.Application.Usecases.Login;
using AtlasSasS.Domain.Abstractions.Login;
using Microsoft.AspNetCore.Mvc;

namespace AtlasSasS.Interface.Http
{
	[ApiController]
	[Route("auth")]
	public sealed class AuthController : ControllerBase
	{
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromServices] RegisterWithPassword uc, [FromBody] RegisterRequest req)
		{
			await uc.HandleAsync(req.Email, req.Password);
			return this.Ok(new
			{
				ok = true
			});
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromServices] LoginWithPassword uc, [FromBody] LoginRequest req)
		{
			var t = await uc.HandleAsync(req.Email, req.Password);
			return Ok(t);
		}

		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh([FromServices] IJwtService jwt, [FromBody] RefreshRequest req)
		{
			var (ok, uid) = await jwt.ValidateRefreshAsync(req.RefreshToken);
			if (!ok)
				return Unauthorized();
			var (a, r, aexp, rexp) = jwt.IssueTokenPair(uid, Array.Empty<string>());
			return Ok(new
			{
				AccessToken = a,
				RefreshToken = r,
				AccessExp = aexp,
				RefreshExp = rexp
			});
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout([FromServices] IJwtService jwt, [FromBody] RefreshRequest req)
		{
			await jwt.RevokeRefreshAsync(req.RefreshToken);
			return Ok(new
			{
				ok = true
			});
		}
	}

	public sealed record RegisterRequest(string Email, string Password);
	public sealed record LoginRequest(string Email, string Password);
	public sealed record RefreshRequest(string RefreshToken);
}
