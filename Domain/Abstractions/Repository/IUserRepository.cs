namespace AtlasSasS.Domain.Abstractions.Repository
{
	public interface IUserRepository
	{
		Task<User?> FindByEmailAsync(string email);
		Task<User?> FindByOAuthAsync(string provider, string providerUserId);
		Task AddAsync(User user);

		Task AddOAuthAccountAsync(Guid userId, OAuthAccount account);
	}
}
