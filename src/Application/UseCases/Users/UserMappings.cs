using Application.UseCases.Users.CQRS.Commands.Create;
using Application.UseCases.Users.CQRS.Commands.Update;
using Application.UseCases.Users.DTOs;
using Domain.Entities.Roles;
using Domain.Entities.Users;

namespace Application.UseCases.Users;

public class UserMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Map Role entity to UserRoleDTO
        config.NewConfig<Role, UserRoleDTO>();

        // Map User to UserDTO with nested Role
        config.NewConfig<User, UserDTO>()
            .Map(dest => dest.Role, src => src.Role);

        config.NewConfig<CreateUserCommand, User>()
            .Ignore(dest => dest.PasswordHashed);

        config.NewConfig<UserUpdateCommand, User>()
            .Ignore(dest => dest.PasswordHashed);
    }
}
