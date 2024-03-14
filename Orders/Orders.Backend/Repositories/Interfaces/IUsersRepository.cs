using Microsoft.AspNetCore.Identity;
using Orders.Shared.DTOs;
using Orders.Shared.Entities;

namespace Orders.Backend.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        Task<User> GetUserAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsync(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);

        Task<SignInResult> LoginAsync(LoginDTO model);

        Task LogoutAsync();
    }
}