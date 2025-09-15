namespace AtlasSasS.Domain.Abstractions.Login
{
	public interface IOAuthClient
	{
		string BuildAuthorizationUrl(string state, string codeChallenge, string redirectUri, IEnumerable<string> scopes);
		Task<(string accessToken, string idToken)> ExchangeCodeAsync(string code, string codeVerifier, string redirectUri);
		Task<OAuthProfile> FetchProfileAsync(string accessToken);

	}
	public sealed record OAuthProfile(string Provider, string ProviderUserId, string? Email, string? Nickname);
}
