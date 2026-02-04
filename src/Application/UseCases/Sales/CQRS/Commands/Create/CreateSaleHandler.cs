using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Sales.DTOs;
using Domain.Entities.Customers;
using Domain.Entities.Inventories;
using Domain.Entities.Sales;
using Domain.Entities.Users;

namespace Application.UseCases.Sales.CQRS.Commands.Create;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, OperationResult<SaleDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSaleHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<SaleDTO>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var rules = new SaleRules(_unitOfWork);

        // Validaciones
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Error(ErrorResult.BadRequest, detail: SaleMessages.Create.EmptyItems);
        }

        if (!await rules.CustomerExistsAsync(request.CustomerId))
        {
            return Result.Error(ErrorResult.NotFound, detail: SaleMessages.Create.CustomerNotFound);
        }

        if (!await rules.UserExistsAsync(request.UserId))
        {
            return Result.Error(ErrorResult.NotFound, detail: SaleMessages.Create.UserNotFound);
        }

        // Validar stock disponible para todos los productos
        var itemsToValidate = request.Items.Select(i => (i.ProductId, i.Quantity)).ToList();
        var (isValid, errorMessage) = await rules.ValidateStockAvailability(itemsToValidate);
        if (!isValid)
        {
            return Result.Error(ErrorResult.BadRequest, detail: errorMessage);
        }

        // Crear la venta
        var sale = new Sale
        {
            CustomerId = request.CustomerId,
            UserId = request.UserId,
            TotalAmount = 0,
            SaleDetails = new List<SaleDetail>()
        };

        decimal totalAmount = 0;

        // Procesar cada item
        foreach (var item in request.Items)
        {
            var product = await rules.GetProductAsync(item.ProductId);
            if (product == null) continue; // Ya validado anteriormente

            var unitPrice = product.UnitPrice;
            var total = item.Quantity * unitPrice;

            // Crear detalle de venta
            var saleDetail = new SaleDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                Total = total
            };

            sale.SaleDetails.Add(saleDetail);
            totalAmount += total;

            // Decrementar inventario
            var inventory = await rules.GetInventoryAsync(item.ProductId);
            if (inventory != null)
            {
                inventory.Quantity -= item.Quantity;
                _unitOfWork.Repository<Inventory>().Update(inventory);
            }
        }

        sale.TotalAmount = totalAmount;

        // Guardar todo en una transacción
        _unitOfWork.Repository<Sale>().Add(sale);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recargar con todas las relaciones para el mapping
        var createdSale = await _unitOfWork.Repository<Sale>().GetByIdAsync(sale.Id, cancellationToken);
        if (createdSale != null)
        {
            createdSale.Customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(createdSale.CustomerId, cancellationToken);
            createdSale.User = await _unitOfWork.Repository<User>().GetByIdAsync(createdSale.UserId, cancellationToken);

            // Cargar los detalles manualmente
            var saleDetails = await _unitOfWork.Repository<SaleDetail>().QueryAsync(
                sd => sd.SaleId == createdSale.Id,
                cancellationToken: cancellationToken
            );

            createdSale.SaleDetails = saleDetails.ToList();

            foreach (var detail in createdSale.SaleDetails)
            {
                detail.Product = await rules.GetProductAsync(detail.ProductId);
            }
        }

        var dto = _mapper.Map<SaleDTO>(createdSale);
        return new OperationResult<SaleDTO>(StatusResult.Created, dto);
    }
}
