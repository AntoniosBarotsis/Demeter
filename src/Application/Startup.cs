using System;
using System.IO;
using System.Reflection;
using Application.Config;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Domain.Services;
using Infrastructure.Data;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;

namespace Application
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(Configuration.GetConnectionString("RedisConnection")));
            services.AddSingleton<ICacheService, RedisCacheService>();

            services.AddDbContext<DemeterDbContext>(opt =>
                {
                    opt.LogTo(Log.Debug);
                    opt.UseNpgsql(Configuration.GetConnectionString("DemeterConnection"));
                }
            );

            services.AddIdentity<User, IdentityRole>(opt => opt.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<DemeterDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.AddControllers(opt => { opt.UseGeneralRoutePrefix("api/"); });

            // services.AddApiVersioning(opt =>
            // {
            //     opt.DefaultApiVersion = ApiVersion.Default;
            //     opt.ReportApiVersions = true;
            // });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Application - v1", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "Application - v2", Version = "v2" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, true); // <== Added the true here, to show the controller description
            });

            services.AddMediatR(Assembly.Load("Infrastructure"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
                using (var scope = scopeFactory.CreateScope())
                {
                    var dbInitializer = scope.ServiceProvider.GetService<IDatabaseSeeder>();
                    if (dbInitializer != null)
                    {
                        dbInitializer.Initialize();
                        dbInitializer.Seed();
                    }
                }

                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Application v1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "Application v2");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}