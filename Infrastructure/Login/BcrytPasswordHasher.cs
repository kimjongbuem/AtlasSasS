using AtlasSasS.Domain.Abstractions.Login;

namespace AtlasSasS.Infrastructure.Login
{

	/// <summary>
	/// 비밀번호를 안전하게 저장하기 위한 느린 해시 함수 bcrypt를 사용한 비밀번호 해시 구현체
	/// </summary>
	public sealed class BcryptPasswordHasher : IPasswordHasher
	{
		public string Hash(string password)
			=> BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

		public bool Verify(string password, string? hash)
			=> BCrypt.Net.BCrypt.Verify(password, hash);
	}
}