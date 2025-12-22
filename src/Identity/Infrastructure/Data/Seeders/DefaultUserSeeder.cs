namespace Infrastructure.Data.Seeders
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Domain.Entities;
    using Domain.Enums;
    using Domain.Interfaces;
    using Application.Interfaces;

    public static class DefaultUserSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
            var passwordService = serviceProvider.GetRequiredService<IPasswordService>();

            // Seed Admin User
            string adminEmail = "admin@example.com";
            string adminUsername = "admin";
            string adminPassword = "Password1!";
            
            var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = adminEmail,
                    Username = adminUsername,
                    Password = passwordService.HashPassword(adminPassword),
                    Role = Role.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userRepository.AddAsync(adminUser);
            }

            // Seed Regular User
            string userEmail = "user@example.com";
            string userUsername = "user";
            string userPassword = "Password1!";
            
            var existingUser = await userRepository.GetByEmailAsync(userEmail);
            if (existingUser == null)
            {
                var regularUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = userEmail,
                    Username = userUsername,
                    Password = passwordService.HashPassword(userPassword),
                    Role = Role.User,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userRepository.AddAsync(regularUser);
            }
        }
    }
}   