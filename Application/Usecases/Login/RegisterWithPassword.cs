using AtlasSasS.Domain.Abstractions.Login;
using AtlasSasS.Domain.Abstractions.Repository;

namespace AtlasSasS.Application.Usecases.Login
{
	public class RegisterWithPassword
	{
		private readonly IUserRepository _users;
		private readonly IPasswordHasher _hasher;
		public RegisterWithPassword(IUserRepository users, IPasswordHasher hasher)
		{
			_users = users;
			_hasher = hasher;
		}

		public async Task HandleAsync(string email, string rawPw)
		{
			var exists = await _users.FindByEmailAsync(email);
			if (exists is not null)
				throw new InvalidOperationException("Email already exists.");
			var user = User.CreateLocal(email, _hasher.Hash(rawPw));
			await _users.AddAsync(user);
		}
	}
}
