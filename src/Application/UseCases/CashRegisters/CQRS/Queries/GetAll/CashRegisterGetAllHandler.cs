using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.CashRegisters.DTOs;
using Domain.Repositories;

namespace Application.UseCases.CashRegisters.CQRS.Queries.GetAll;

public sealed class CashRegisterGetAllHandler : IRequestHandler<CashRegisterGetAllQuery, OperationResult<IEnumerable<CashRegisterDTO>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CashRegisterGetAllHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<IEnumerable<CashRegisterDTO>>> Handle(
        CashRegisterGetAllQuery request,
        CancellationToken cancellationToken)
    {
        var cashRegisters = await _unitOfWork.CashRegisters.GetAllWithDetailsAsync(cancellationToken);

        var dtos = _mapper.Map<IEnumerable<CashRegisterDTO>>(cashRegisters);
        return Result.Success(dtos);
    }
}
