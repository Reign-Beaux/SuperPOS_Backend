using Application.Events;
using Application.Interfaces.Persistence;
using Domain.Events.Sales;

namespace Application.EventHandlers;

/// <summary>
/// Handles sale cancelled events by restoring inventory for all items in the sale.
/// </summary>
public class SaleCancelledEventHandler : IEventHandler<SaleCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public SaleCancelledEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SaleCancelledEvent notification, CancellationToken cancellationToken)
    {
        // Restore inventory for each item in the cancelled sale
        foreach (var item in notification.ItemsToRestore)
        {
            var inventory = await _unitOfWork.Inventories.GetByProductIdAsync(item.ProductId, cancellationToken);

            if (inventory != null)
            {
                // Add stock back to inventory
                inventory.AddStock(item.Quantity);
                _unitOfWork.Inventories.Update(inventory);
            }
        }

        // Save all inventory changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
