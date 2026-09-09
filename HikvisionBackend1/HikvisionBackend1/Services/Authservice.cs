using HikvisionBackend1.Data;
using HikvisionBackend1.DTOs;
using HikvisionBackend1.Interface;
using HikvisionBackend1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HikvisionBackend1.Services
{
    public class Authservice:IAuth
    {
       
        private readonly AppDbContext Context;
        private readonly PasswordHasher<UserModel> Hash;
        public Authservice(AppDbContext _Context)
        {
            Context= _Context;
            Hash= new PasswordHasher<UserModel>();
        }
        //Register
        public async Task<ResponceDTO> RegisterUser(UserModel Model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Model.Name))
                {
                    return new ResponceDTO(false, "Name is required");
                }
                if (string.IsNullOrWhiteSpace(Model.Email))
                {
                    return new ResponceDTO(false, "Email is required");
                }
                if (string.IsNullOrWhiteSpace(Model.PasswordHash))
                {
                    return new ResponceDTO(false, "Password is required");
                }

                var existing_user = Context.Users.FirstOrDefault(x => x.Email == Model.Email);
                if (existing_user != null)
                {
                    return new ResponceDTO(false, "Email already exist");
                }
                var Password_Hash1 = Hash.HashPassword(Model, Model.PasswordHash);

                Model.PasswordHash = Password_Hash1;
                Model.IsActive = false;
                Model.CreatedAt = DateTime.UtcNow;
                Model.UpdatedAt = DateTime.UtcNow;
                await Context.Users.AddAsync(Model);
                await Context.SaveChangesAsync();
                return new ResponceDTO(true, "Register Successfuly");
            }
            catch (Exception ex)
            {
                Console.WriteLine("REGISTER ERROR:");
                Console.WriteLine(ex.ToString());

                return new ResponceDTO(false, ex.Message);
            }

        }


       // login
        public async Task<ResponceDTO> LoginUser(string email, string password)
        {
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new ResponceDTO(false, "Email is required");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return new ResponceDTO(false, "Password is required");
                }

                // Find user by email
                var user = await Context.Users
                    .FirstOrDefaultAsync(x => x.Email == email);

                if (user == null)
                {
                    return new ResponceDTO(false, "Invalid email or password");
                }

                // Check whether account is active
                if (!user.IsActive)
                {
                    return new ResponceDTO(false, "Your account is not active");
                }

                // Verify entered password with stored hash
                var passwordValid = Hash.VerifyHashedPassword(user, user.PasswordHash, password);

                if (passwordValid != PasswordVerificationResult.Success)
                {
                    return new ResponceDTO(false, "Invalid email or password");
                }

                return new ResponceDTO(true, "Login Successful");
            }
          
        }
    }
}
