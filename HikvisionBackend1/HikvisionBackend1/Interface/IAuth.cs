using HikvisionBackend1.DTOs;
using HikvisionBackend1.Models;

namespace HikvisionBackend1.Interface
{
    public interface IAuth
    {
        Task<ResponceDTO> LoginUser(string email, string password);
        Task<ResponceDTO> RegisterUser(UserModel Model);
    }
}
