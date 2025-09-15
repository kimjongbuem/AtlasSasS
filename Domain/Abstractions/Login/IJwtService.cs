namespace AtlasSasS.Domain.Abstractions.Login
{
	public interface IJwtService
	{
		(string accessToken, string refreshToken, DateTimeOffset accessExp, DateTimeOffset refreshExp)
			IssueTokenPair(Guid userId, IEnumerable<string> rolesClaims);
		Task<(bool ok, Guid userId)> ValidateRefreshAsync(string refreshToken);
		Task RevokeRefreshAsync(string refreshToken); // 회전/폐기
	}
}
