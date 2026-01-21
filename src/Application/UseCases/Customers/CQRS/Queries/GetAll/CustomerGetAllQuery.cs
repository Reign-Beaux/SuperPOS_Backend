using Application.DesignPatterns.OperationResults;
using Application.UseCases.Customers.DTOs;
using Application.DesignPatterns.Mediators.Interfaces;

namespace Application.UseCases.Customers.CQRS.Queries.GetAll;

public sealed record CustomerGetAllQuery : IRequest<OperationResult<IEnumerable<CustomerDTO>>>;
