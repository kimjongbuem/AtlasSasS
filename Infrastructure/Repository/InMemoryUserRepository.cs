namespace AtlasSasS.Infrastructure.Repository
{
	using AtlasSasS.Domain.Abstractions.Repository;
	using System.Collections.Concurrent;

	public sealed class InMemoryUserRepository : IUserRepository
	{
		// 메인 저장소: UserId → User (불변 객체 취급 권장)
		private readonly ConcurrentDictionary<Guid, User> _users = new();

		// 보조 인덱스: 이메일(정규화) → UserId
		private readonly ConcurrentDictionary<string, Guid> _emailIndex = new(StringComparer.OrdinalIgnoreCase);

		// 보조 인덱스: (provider|providerUserId) → UserId
		private readonly ConcurrentDictionary<string, Guid> _oauthIndex = new(StringComparer.Ordinal);

		private static string NormalizeEmail(string email)
			=> email.Trim().ToLowerInvariant();

		private static string OKey(string provider, string providerUserId)
			=> $"{provider}|{providerUserId}";

		public Task<User?> FindByEmailAsync(string email)
		{
			var key = NormalizeEmail(email);
			return Task.FromResult(_emailIndex.TryGetValue(key, out var id) && _users.TryGetValue(id, out var u) ? u : null);
		}

		public Task<User?> FindByOAuthAsync(string provider, string providerUserId)
		{
			var key = OKey(provider, providerUserId);
			return Task.FromResult(_oauthIndex.TryGetValue(key, out var id) && _users.TryGetValue(id, out var u) ? u : null);
		}

		public Task AddAsync(User user)
		{
			// 유니크 제약: Email
			if (!string.IsNullOrWhiteSpace(user.Email))
			{
				var ekey = NormalizeEmail(user.Email);
				// 이미 같은 이메일이 있으면 충돌
				if (_emailIndex.ContainsKey(ekey))
					throw new InvalidOperationException("Email already exists.");
			}

			if (!_users.TryAdd(user.Id, user))
				throw new InvalidOperationException("User insert failed.");

			if (!string.IsNullOrWhiteSpace(user.Email))
			{
				var ekey = NormalizeEmail(user.Email!);
				_emailIndex[ekey] = user.Id;
			}

			// 소셜 계정 동시 추가가 들어오면 인덱싱
			foreach (var acc in user.OAuthAccounts)
			{
				var ok = OKey(acc.Provider, acc.ProviderUserId);
				if (!_oauthIndex.TryAdd(ok, user.Id))
				{
					// 중복 소셜 키 방지
					_users.TryRemove(user.Id, out _);
					if (!string.IsNullOrWhiteSpace(user.Email))
						_emailIndex.TryRemove(NormalizeEmail(user.Email!), out _);
					throw new InvalidOperationException("OAuth account already linked to another user.");
				}
			}

			return Task.CompletedTask;
		}

		public Task AddOAuthAccountAsync(Guid userId, OAuthAccount account)
		{
			// 1) 유저 존재 확인
			if (!_users.TryGetValue(userId, out var user))
				throw new KeyNotFoundException("User not found.");

			// 2) 소셜 키 유니크 확인
			var key = OKey(account.Provider, account.ProviderUserId);
			if (_oauthIndex.ContainsKey(key))
				throw new InvalidOperationException("OAuth account already linked.");

			// 3) 새 User 인스턴스로 교체(불변 취급: 리스트 복사)
			var newUser = new User
			{
				Id = user.Id,
				Email = user.Email,
				PasswordHash = user.PasswordHash,
				CreatedAt = user.CreatedAt
			};
			foreach (var a in user.OAuthAccounts)
				newUser.LinkOAuth(a);
			newUser.LinkOAuth(account);

			// 4) 교체(원자적 업데이트)
			_users[userId] = newUser;
			_oauthIndex[key] = userId;

			return Task.CompletedTask;
		}
	}
}
