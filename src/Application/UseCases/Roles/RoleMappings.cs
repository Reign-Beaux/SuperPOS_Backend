using Application.UseCases.Roles.CQRS.Commands.Create;
using Application.UseCases.Roles.CQRS.Commands.Update;
using Application.UseCases.Roles.DTOs;
using Domain.Entities.Roles;

namespace Application.UseCases.Roles;

public class RoleMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Role, RoleDTO>();
        config.NewConfig<CreateRoleCommand, Role>();
        config.NewConfig<RoleUpdateCommand, Role>();
    }
}
