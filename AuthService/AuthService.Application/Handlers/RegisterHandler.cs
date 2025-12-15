using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Application.Handlers;

public class RegisterHandler
{
    private readonly IUserRepository _repo;
    private readonly IEventProducer _producer;

    public RegisterHandler(IUserRepository repo, IEventProducer producer)
    {
        _repo = repo;
        _producer = producer;
    }

    public async Task ExecuteAsync(string name, string phone, string email, string passwordHash)
    {
        var exists = await _repo.GetByEmailAsync(email);
        if (exists != null)
            throw new Exception("User already exists");

        var user = new User
        {
            Name = name,
            Phone = phone,
            Email = email,
            PasswordHash = passwordHash
        };

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        await _producer.PublishAsync("user.registered", new
        {
            UserId = user.Id,
            user.Name,
            user.Email,
            user.Phone,
            PasswordHash = user.PasswordHash, // Sync password hash for password change functionality
            Timestamp = DateTime.UtcNow
        });
    }
}

