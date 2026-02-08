using Application.UseCases.CashRegisters.DTOs;
using Domain.Entities.CashRegisters;

namespace Application.UseCases.CashRegisters;

public class CashRegisterMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CashRegister, CashRegisterDTO>()
            .Map(dest => dest.UserName, src => src.User != null ? src.User.Name : string.Empty);
    }
}
