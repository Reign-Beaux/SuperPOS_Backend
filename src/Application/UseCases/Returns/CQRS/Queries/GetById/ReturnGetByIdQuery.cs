using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Returns.DTOs;

namespace Application.UseCases.Returns.CQRS.Queries.GetById;

public record ReturnGetByIdQuery(Guid Id) : IRequest<OperationResult<ReturnDTO>>;
