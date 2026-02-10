using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.CashRegisters.DTOs;
using Application.UseCases.Sales.DTOs;
using Domain.Entities.CashRegisters;
using Domain.Entities.Sales;
using Domain.Repositories;

namespace Application.UseCases.CashRegisters.CQRS.Commands.Create;

public sealed class CreateCashRegisterHandler : IRequestHandler<CreateCashRegisterCommand, OperationResult<CashRegisterReportDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCashRegisterHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<CashRegisterReportDTO>> Handle(
        CreateCashRegisterCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validar que el usuario existe
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Error(
                ErrorResult.NotFound,
                detail: CashRegisterMessages.Create.UserNotFound);
        }

        // 2. Validar que OpeningDate < ClosingDate
        if (request.OpeningDate >= request.ClosingDate)
        {
            return Result.Error(
                ErrorResult.BadRequest,
                detail: CashRegisterMessages.Create.InvalidDateRange);
        }

        // 3. Validar que las fechas no sean futuras
        if (request.ClosingDate > DateTime.UtcNow)
        {
            return Result.Error(
                ErrorResult.BadRequest,
                detail: CashRegisterMessages.Create.FutureDateNotAllowed);
        }

        // 4. Validar que los montos no sean negativos
        if (request.InitialCash < 0)
        {
            return Result.Error(
                ErrorResult.BadRequest,
                detail: CashRegisterMessages.Create.NegativeInitialCash);
        }

        if (request.FinalCash < 0)
        {
            return Result.Error(
                ErrorResult.BadRequest,
                detail: CashRegisterMessages.Create.NegativeFinalCash);
        }

        // 5. Obtener todas las ventas del periodo
        var sales = await _unitOfWork.Sales.GetByDateRangeAsync(
            request.OpeningDate,
            request.ClosingDate,
            cancellationToken);

        // 6. Cargar detalles completos de cada venta
        var salesWithDetails = new List<Sale>();
        foreach (var sale in sales)
        {
            var saleWithDetails = await _unitOfWork.Sales.GetByIdWithDetailsAsync(
                sale.Id,
                cancellationToken);

            if (saleWithDetails != null)
            {
                salesWithDetails.Add(saleWithDetails);
            }
        }

        // 7. Calcular totales
        var totalSales = salesWithDetails.Sum(s => s.TotalAmount);
        var totalTransactions = salesWithDetails.Count;
        var totalItemsSold = salesWithDetails.Sum(s => s.GetTotalQuantity());

        // 8. Crear entidad CashRegister
        var cashRegister = CashRegister.Create(
            userId: request.UserId,
            openingDate: request.OpeningDate,
            closingDate: request.ClosingDate,
            initialCash: request.InitialCash,
            finalCash: request.FinalCash,
            totalSales: totalSales,
            totalTransactions: totalTransactions,
            totalItemsSold: totalItemsSold,
            notes: request.Notes
        );

        // 9. Guardar en base de datos
        _unitOfWork.CashRegisters.Add(cashRegister);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 10. Cargar la entidad creada con detalles (User)
        var createdCashRegister = await _unitOfWork.CashRegisters.GetByIdWithDetailsAsync(
            cashRegister.Id,
            cancellationToken);

        if (createdCashRegister == null)
        {
            return Result.Error(
                ErrorResult.BadRequest,
                detail: "Error al cargar el corte de caja creado");
        }

        // 11. Mapear a DTOs
        var cashRegisterDTO = _mapper.Map<CashRegisterDTO>(createdCashRegister);
        var saleDTOs = _mapper.Map<List<SaleDTO>>(salesWithDetails);

        // 12. Crear y retornar reporte completo
        var report = new CashRegisterReportDTO(
            CashRegister: cashRegisterDTO,
            Sales: saleDTOs
        );

        return Result.Success(report);
    }
}
