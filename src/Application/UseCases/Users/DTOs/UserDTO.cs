namespace Application.UseCases.Users.DTOs;

public class UserDTO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string FirstLastname { get; set; }
    public string? SecondLastname { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
}
