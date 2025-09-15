// Domain/Entities/User.cs (필요 최소)
public sealed class User
{
	public Guid Id { get; init; } = Guid.NewGuid();
	public string? Email
	{
		get; set;
	}              // 소셜-only면 null 허용
	public string? PasswordHash
	{
		get; set;
	}       // 소셜-only면 null
	public bool IsActive { get; private set; } = true;
	public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

	private readonly List<OAuthAccount> _oauth = new();
	public IReadOnlyList<OAuthAccount> OAuthAccounts => _oauth;

	public static User CreateLocal(string email, string passwordHash)
		=> new()
		{ Email = email, PasswordHash = passwordHash };

	public static User CreateSocial(string? email = null)
		=> new()
		{ Email = email, PasswordHash = null };

	public void LinkOAuth(OAuthAccount acc) => _oauth.Add(acc);

}

public sealed class OAuthAccount
{
	public string Provider { get; init; } = default!;      // "kakao" | "naver"
	public string ProviderUserId { get; init; } = default!;
	public string? Email
	{
		get; init;
	}
	public string? Nickname
	{
		get; init;
	}
}
