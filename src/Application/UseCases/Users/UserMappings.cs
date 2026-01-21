using Application.UseCases.Users.CQRS.Commands.Create;
using Application.UseCases.Users.CQRS.Commands.Update;
using Application.UseCases.Users.DTOs;
using Domain.Entities.Users;

namespace Application.UseCases.Users;

public class UserMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserDTO>();
        config.NewConfig<CreateUserCommand, User>()
            .Ignore(dest => dest.PasswordHashed);
        config.NewConfig<UserUpdateCommand, User>()
            .Ignore(dest => dest.PasswordHashed);
    }
}
