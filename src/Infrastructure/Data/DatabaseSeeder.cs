using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Infrastructure.Data
{
    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly UserManager<User> _userManager;

        public DatabaseSeeder(IServiceScopeFactory scopeFactory, ILogger logger, UserManager<User> userManager)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _userManager = userManager;
        }

        public void Seed()
        {
            _logger.Information("Seeding Database...");
            
            // Create users
            var order = new Order {Id = 1, Items = null, Price = 42};
            var pastOrders = new List<Order>{ order };
            var users = new List<User>
            {
                new("Tony", "test@test.com") {PastOrders = pastOrders}
            };
            
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<DemeterDbContext>();

            if (context is null)
            {
                _logger.Fatal("Context is null");
                throw new Exception("Context is null");
            }
            
            // Clear all users from the database
            context.Users.RemoveRange(context.Users);
            context.Orders.RemoveRange(context.Orders);
            context.SaveChanges();
            
            // Push the users from the list
            foreach (var user in users)
            {
                var succeeded = _userManager.CreateAsync(user, "password").Result.Succeeded;
                
                if (!succeeded)
                    Log.Error($"Failed to add user {user}");
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