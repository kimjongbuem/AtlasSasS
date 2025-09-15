using AtlasSasS.Domain.Abstractions.Login;
using AtlasSasS.Domain.Abstractions.Repository;
using Microsoft.AspNetCore.Identity;

namespace AtlasSasS.Application.Usecases.Login
{
	// Application/UseCases/LoginWithPassword.cs
	public sealed class LoginWithPassword
	{
		private readonly IUserRepository _users;
		private readonly IPasswordHasher _hasher;
		private readonly IJwtService _jwt;

		public LoginWithPassword(IUserRepository users, IPasswordHasher hasher, IJwtService jwt)
		{
			_users = users;
			_hasher = hasher;
			_jwt = jwt;
		}

		public async Task<TokenPairResult> HandleAsync(string email, string password)
		{
			User? user = await _users.FindByEmailAsync(email);
			if (user is null)
				throw new UnauthorizedAccessException("Invalid credentials");

			if (!_hasher.Verify(password, user?.PasswordHash))
				throw new UnauthorizedAccessException("Invalid credentials");

#pragma warning disable CS8602 // null 가능 참조에 대한 역참조입니다.
			var (access, refresh, aexp, rexp) = _jwt.IssueTokenPair(user.Id, rolesClaims: Array.Empty<string>());
#pragma warning restore CS8602 // null 가능 참조에 대한 역참조입니다.
			return new TokenPairResult(access, refresh, aexp, rexp);
		}
	}

	public sealed record TokenPairResult(string AccessToken, string RefreshToken, DateTimeOffset AccessExp, DateTimeOffset RefreshExp);

}
