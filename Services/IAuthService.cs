using ERPDemo.Models;

namespace ERPDemo.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
        Task<User?> ValidateUserAsync(string usernameOrEmail, string password);
        Task UpdateLastLoginAsync(int userId);
    }
}