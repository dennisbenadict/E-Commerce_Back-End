namespace UserService.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }                 
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

