using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Customers.DTOs;

namespace Application.UseCases.Customers.CQRS.Commands.Create;

public record CreateCustomerCommand(
    string Name,
    string FirstLastname,
    string? SecondLastname,
    string? Phone,
    string? Email,
    DateTime? BirthDate
) : IRequest<OperationResult<CustomerDTO>>;
