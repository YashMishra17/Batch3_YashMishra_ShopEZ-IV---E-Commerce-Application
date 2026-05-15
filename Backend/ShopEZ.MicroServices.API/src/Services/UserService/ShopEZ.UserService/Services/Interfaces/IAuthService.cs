using ShopEZ.UserService.DTOs;

namespace ShopEZ.UserService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO dto);
    }
}
