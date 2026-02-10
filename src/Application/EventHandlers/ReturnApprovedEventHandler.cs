using Application.Events;
using Application.Interfaces.Persistence;
using Domain.Events.Returns;

namespace Application.EventHandlers;

/// <summary>
/// Handles return approved events by restoring inventory for returned items.
/// </summary>
public class ReturnApprovedEventHandler : IEventHandler<ReturnApprovedEvent>
{
    private readonly IUnitOfWork _unitOfWork;

    public ReturnApprovedEventHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReturnApprovedEvent notification, CancellationToken cancellationToken)
    {
        // Restore inventory for each returned item
        foreach (var item in notification.ItemsToRestock)
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
