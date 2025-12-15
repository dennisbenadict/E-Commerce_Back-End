using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IEventProducer _producer;

        public CategoryService(ICategoryRepository repo, IEventProducer producer)
        {
            _repo = repo;
            _producer = producer;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            await _repo.AddAsync(category);
            await _repo.SaveChangesAsync();

            await _producer.PublishAsync("category.created", new
            {
                category.Id,
                category.Name,
                Timestamp = DateTime.UtcNow
            });
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cat = await _repo.GetByIdAsync(id);
            if (cat == null) return false;

            await _repo.DeleteAsync(id);
            await _repo.SaveChangesAsync();

            await _producer.PublishAsync("category.deleted", new
            {
                CategoryId = id,
                Timestamp = DateTime.UtcNow
            });
            return true;
        }

        public async Task<Category?> UpdateAsync(Category category)
        {
            var existing = await _repo.GetByIdAsync(category.Id);
            if (existing == null) return null;

            existing.Name = category.Name;

            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();

            await _producer.PublishAsync("category.updated", new
            {
                existing.Id,
                existing.Name,
                Timestamp = DateTime.UtcNow
            });
            return existing;
        }
    }
}
