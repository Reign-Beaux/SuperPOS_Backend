using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Returns.DTOs;

namespace Application.UseCases.Returns.CQRS.Queries.GetAll;

public record ReturnGetAllQuery : IRequest<OperationResult<List<ReturnDTO>>>;
