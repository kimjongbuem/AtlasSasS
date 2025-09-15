namespace AtlasSasS.Domain.Abstractions.Login
{
	public interface IPasswordHasher
	{
		string Hash(string rawPw);
		bool Verify(string password, string? passwordHash);
	}
}
