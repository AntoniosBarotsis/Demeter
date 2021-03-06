using System;
using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Data
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IUserManager _userManager;

        public DatabaseSeeder(IServiceScopeFactory scopeFactory, ILogger logger, IUserManager userManager)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _userManager = userManager;
        }

        public void Seed()
        {
            _logger.Information("Seeding Database...");

            // Create users
            var item = new MenuItem("itemName", "itemDesc", 42, "");
            var order = new Order { Id = 1, Items = new List<MenuItem>{item}, Price = 42 };
            var u1 = new User("Tony", "test@test.com");
            u1.PastOrders.Add(order);

            var users = new List<User>
            {
                u1
            };

            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<DemeterDbContext>();

            if (context is null)
            {
                _logger.Fatal("Context is null");
                throw new Exception("Context is null");
            }

            // Clear all users from the database
            context.RefreshTokens.RemoveRange(context.RefreshTokens);
            context.Users.RemoveRange(context.Users);
            context.Orders.RemoveRange(context.Orders);
            context.MenuItems.RemoveRange(context.MenuItems);
            context.SaveChanges();

            // Push the users from the list
            foreach (var user in users)
            {
                var succeeded = _userManager.CreateAsync(user, "password").Result.Succeeded;

                if (!succeeded)
                    Log.Error("Failed to add user {User}", user.UserName);
            }

            _logger.Information("Done");
        }

        public void Initialize()
        {
            _logger.Information("Applying Database Migrations...");

            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<DemeterDbContext>();
            context?.Database.Migrate();

            _logger.Information("Done");
        }
    }
}