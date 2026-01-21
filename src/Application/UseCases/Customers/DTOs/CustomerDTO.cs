namespace Application.UseCases.Customers.DTOs;

public class CustomerDTO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string FirstLastname { get; set; }
    public string? SecondLastname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? BirthDate { get; set; }
}
