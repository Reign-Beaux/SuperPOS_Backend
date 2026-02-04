namespace Application.UseCases.Products.DTOs;

public class ProductDTO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public decimal UnitPrice { get; set; }
}
