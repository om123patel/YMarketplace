using Identity.Application.Services.Interfaces;

namespace Identity.Infrastructure.Services
{
    public class PasswordService : IPasswordHasher
    {
        public string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        public bool VerifyPassword(string password, string hash)
            => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
