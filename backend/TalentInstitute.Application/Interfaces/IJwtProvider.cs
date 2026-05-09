namespace TalentInstitute.Application.Interfaces;

public interface IJwtProvider
{
    string Generate(string userId, string email, string role);
}
