using BCrypt.Net;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Security;

public class BcryptPasswordHasher : IPasswordHasher
{
	public string Hash(string password)
	{
		// WorkFactor 10 is secure + fast enough
		return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
	}

	public bool Verify(string hash, string password)
	{
		return BCrypt.Net.BCrypt.Verify(password, hash);
	}
}
