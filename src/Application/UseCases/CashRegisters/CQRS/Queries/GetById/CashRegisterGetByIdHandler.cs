using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.CashRegisters.DTOs;
using Domain.Entities.CashRegisters;
using Domain.Repositories;

namespace Application.UseCases.CashRegisters.CQRS.Queries.GetById;

public sealed class CashRegisterGetByIdHandler : IRequestHandler<CashRegisterGetByIdQuery, OperationResult<CashRegisterDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CashRegisterGetByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<CashRegisterDTO>> Handle(
        CashRegisterGetByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cashRegister = await _unitOfWork.CashRegisters.GetByIdWithDetailsAsync(
            request.Id,
            cancellationToken);

        if (cashRegister == null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: CashRegisterMessages.NotFound.WithId(request.Id));
        }

        var dto = _mapper.Map<CashRegisterDTO>(cashRegister);
        return Result.Success(dto);
    }
}
