namespace Application.UseCases.Products.DTOs;

/// <summary>
/// DTO for paginated product results with metadata.
/// </summary>
public record PagedProductsDTO(
    List<ProductDTO> Items,
    int TotalCount,
    int PageIndex,
    int PageSize,
    int TotalPages
);
