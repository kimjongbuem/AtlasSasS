using AtlasSasS.Domain.Abstractions.Login;

namespace AtlasSasS.Application.Usecases.Login
{
	public sealed class BuildOAuthAuthorizationUrl
	{
		private readonly IOAuthClient _client;
		public BuildOAuthAuthorizationUrl(IOAuthClient client) => _client = client;

		public string Handle(string state, string codeChallenge, string redirectUri)
			=> _client.BuildAuthorizationUrl(state, codeChallenge, redirectUri, new[] { "profile", "account_email" });
	}
}
