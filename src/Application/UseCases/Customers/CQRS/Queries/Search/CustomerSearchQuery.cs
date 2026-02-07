using Application.DesignPatterns.OperationResults;
using Application.UseCases.Customers.DTOs;
using Application.DesignPatterns.Mediators.Interfaces;

namespace Application.UseCases.Customers.CQRS.Queries.Search;

public sealed record CustomerSearchQuery(string Term) : IRequest<OperationResult<IEnumerable<CustomerDTO>>>;
