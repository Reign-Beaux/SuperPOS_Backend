namespace Application.UseCases.Articles.DTOs;

public class ArticleDTO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Barcode { get; set; }
}
