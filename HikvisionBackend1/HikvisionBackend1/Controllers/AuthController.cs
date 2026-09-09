using Microsoft.AspNetCore.Http;
using BCrypt.Net;
using HikvisionBackend1.Data;
using HikvisionBackend1.DTOs;
using HikvisionBackend1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HikvisionBackend1.Interface;

namespace HikvisionBackend1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuth Auth;
        public AuthController(IAuth auth)
        {
            Auth= auth;
        }

        [HttpPost("register")]
        public async Task<ResponceDTO> RegisterUser(UserModel Model)
        {
            return await Auth.RegisterUser(Model);
        }


        [HttpPost("login")]
        public async Task<ResponceDTO> LoginUser(string email,string password)
        {
            return await Auth.LoginUser(email, password);
        }

    }
}
