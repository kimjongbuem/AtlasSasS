namespace AtlasSasS.Infrastructure.Login
{
	public sealed class JwtOptions
	{
		public string Issuer { get; set; } = "atlas-auth";
		public string Audience { get; set; } = "atlas-api";
		public int AccessMinutes { get; set; } = 20;
		public int RefreshDays { get; set; } = 14;
		public string HmacSecret { get; set; } = "dev-very-long-secret-change-me"; // 개발용
	}
}
