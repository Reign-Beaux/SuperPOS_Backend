using Application.UseCases.Products.CQRS.Commands.Create;
using Application.UseCases.Products.CQRS.Commands.Update;
using Application.UseCases.Products.DTOs;
using Domain.Entities.Products;

namespace Application.UseCases.Products;

public class ProductMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, ProductDTO>();
        config.NewConfig<CreateProductCommand, Product>();
        config.NewConfig<ProductUpdateCommand, Product>();
    }
}
