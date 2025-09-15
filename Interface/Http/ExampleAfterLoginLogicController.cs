using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AtlasSasS.Interface.Http
{
	[ApiController]
	[Route("/after/logic/example")]
	public sealed class ExampleAfterLoginLogicController : ControllerBase
	{
		[HttpGet]
		[Authorize]  // Access 토큰 필요
		public IActionResult GetProfile()
		{
			var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
			return this.Ok(new
			{
				userId,
				hello = "atlas"
			});
		}
	}
}
