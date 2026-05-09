using BCrypt.Net;
using TalentInstitute.Application.Interfaces;

namespace TalentInstitute.Infrastructure.Authentication;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
