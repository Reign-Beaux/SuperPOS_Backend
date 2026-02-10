using Application.UseCases.Returns.DTOs;
using Domain.Entities.Returns;
using Mapster;

namespace Application.UseCases.Returns;

public class ReturnMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Return, ReturnDTO>()
            .Map(dest => dest.CustomerName, src => src.Customer != null ? src.Customer.Name : string.Empty)
            .Map(dest => dest.ProcessedByUserName, src => src.ProcessedByUser != null ? src.ProcessedByUser.Name : string.Empty);

        config.NewConfig<ReturnDetail, ReturnDetailDTO>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty);
    }
}
