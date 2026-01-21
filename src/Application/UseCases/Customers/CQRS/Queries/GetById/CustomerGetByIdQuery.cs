using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Customers.DTOs;

namespace Application.UseCases.Customers.CQRS.Queries.GetById;

public sealed record CustomerGetByIdQuery(Guid Id) : IRequest<OperationResult<CustomerDTO>>;
