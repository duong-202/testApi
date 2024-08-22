using DemoApiDotNet.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoApiDotNet.Domain.InterfaceRepositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByUsername(string username);
        Task<User> GetUserByPhoneNumber(string phoneNumber);
        Task AddRolesToUserAsync(User user, List<string> listRoles);
        Task<IEnumerable<string>> GetRolesOfUserAsync(User user);
        Task DeleteRolesAsync(User user, List<string> roles);
    }
}
