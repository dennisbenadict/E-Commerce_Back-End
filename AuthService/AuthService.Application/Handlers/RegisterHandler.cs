using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Application.Handlers;

public class RegisterHandler
{
    private readonly IUserRepository _repo;

    public RegisterHandler(IUserRepository repo)
    {
        _repo = repo;
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
    }
}

