using System.Threading;
using System.Threading.Tasks;

namespace TalentInstitute.Application.Interfaces;

public interface IAuthService
{
    Task<string> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
