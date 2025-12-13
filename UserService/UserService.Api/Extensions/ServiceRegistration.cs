using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Security;
using UserService.Infrastructure.RabbitMQ;


namespace UserService.Api.Extensions;

public static class ServiceRegistration
{
    public static void AddUserService(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<UserServiceDbContext>(opts =>
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();

        // Application services
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // RabbitMQ → bind interface → pass host name correctly
        services.AddSingleton<IEventProducer>(sp =>
        {
            var rabbitHost = config.GetValue<string>("RabbitMq:Host") ?? "rabbitmq";
            return new RabbitMqProducer(rabbitHost);
        });
    }
}

