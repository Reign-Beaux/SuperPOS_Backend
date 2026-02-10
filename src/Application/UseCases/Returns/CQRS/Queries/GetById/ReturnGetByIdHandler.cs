using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.UseCases.Returns.DTOs;
using Domain.Entities.Returns;
using MapsterMapper;

namespace Application.UseCases.Returns.CQRS.Queries.GetById;

public class ReturnGetByIdHandler : IRequestHandler<ReturnGetByIdQuery, OperationResult<ReturnDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReturnGetByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<ReturnDTO>> Handle(ReturnGetByIdQuery request, CancellationToken cancellationToken)
    {
        var returnEntity = await _unitOfWork.Returns.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (returnEntity == null)
            return Result.Error(ErrorResult.NotFound, detail: ReturnMessages.NotFound);

        var dto = _mapper.Map<ReturnDTO>(returnEntity);
        return Result.Success(dto);
    }
}
