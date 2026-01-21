namespace Application.UseCases.Roles.DTOs;

public class RoleDTO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}
