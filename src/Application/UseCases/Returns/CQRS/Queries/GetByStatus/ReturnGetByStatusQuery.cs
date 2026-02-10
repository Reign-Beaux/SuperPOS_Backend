using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Returns.DTOs;
using Domain.Entities.Returns;

namespace Application.UseCases.Returns.CQRS.Queries.GetByStatus;

public record ReturnGetByStatusQuery(ReturnStatus Status) : IRequest<OperationResult<List<ReturnDTO>>>;
