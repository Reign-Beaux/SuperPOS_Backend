using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Customers.CQRS.Commands.Update;

public sealed record CustomerUpdateCommand(
    Guid Id,
    string Name,
    string FirstLastname,
    string? SecondLastname,
    string? Phone,
    string? Email,
    DateTime? BirthDate) : IRequest<OperationResult<VoidResult>>;
