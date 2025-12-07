using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IUserProfileRepository
{
	Task<UserProfile?> GetByIdAsync(int id);
	Task AddAsync(UserProfile profile);
	Task UpdateAsync(UserProfile profile);
	Task SaveChangesAsync();
}
