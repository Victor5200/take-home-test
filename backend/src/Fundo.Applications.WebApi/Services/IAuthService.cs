using Fundo.Applications.WebApi.DTOs;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto registerDto);
        Task<AuthResponseDto> Login(LoginDto loginDto);
    }
}
