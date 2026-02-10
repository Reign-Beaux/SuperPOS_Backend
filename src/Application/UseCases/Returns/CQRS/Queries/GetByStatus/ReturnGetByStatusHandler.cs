using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.UseCases.Returns.DTOs;
using MapsterMapper;

namespace Application.UseCases.Returns.CQRS.Queries.GetByStatus;

public class ReturnGetByStatusHandler : IRequestHandler<ReturnGetByStatusQuery, OperationResult<List<ReturnDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReturnGetByStatusHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<List<ReturnDTO>>> Handle(ReturnGetByStatusQuery request, CancellationToken cancellationToken)
    {
        var returns = await _unitOfWork.Returns.GetByStatusAsync(request.Status, cancellationToken);
        var dtos = _mapper.Map<List<ReturnDTO>>(returns);
        return Result.Success(dtos);
    }
}
