using Application.UseCases.Customers.CQRS.Commands.Create;
using Application.UseCases.Customers.CQRS.Commands.Update;
using Application.UseCases.Customers.DTOs;
using Domain.Entities.Customers;

namespace Application.UseCases.Customers;

public class CustomerMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Customer, CustomerDTO>();
        config.NewConfig<CreateCustomerCommand, Customer>();
        config.NewConfig<CustomerUpdateCommand, Customer>();
    }
}
