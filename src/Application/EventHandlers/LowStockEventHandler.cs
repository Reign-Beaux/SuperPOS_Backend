using Application.Events;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Events.Inventories;

namespace Application.EventHandlers;

/// <summary>
/// Handles low stock events by sending email alerts to managers.
/// </summary>
public class LowStockEventHandler : IEventHandler<LowStockEvent>
{
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public LowStockEventHandler(
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LowStockEvent notification, CancellationToken cancellationToken)
    {
        // 1. Get product details
        var product = await _unitOfWork.Products.GetByIdAsync(notification.ProductId, cancellationToken);
        if (product == null)
            return;

        // 2. Get all users with "Gerente" role
        var managers = await GetManagersAsync(cancellationToken);

        if (!managers.Any())
            return; // No managers to notify

        // 3. Send email to each manager
        foreach (var manager in managers)
        {
            if (!string.IsNullOrEmpty(manager.Email))
            {
                await _emailService.SendLowStockAlertAsync(
                    manager.Email,
                    product.Name,
                    product.Barcode,
                    notification.CurrentQuantity,
                    cancellationToken);
            }
        }
    }

    private async Task<List<Domain.Entities.Users.User>> GetManagersAsync(CancellationToken cancellationToken)
    {
        // Get "Gerente" role
        var allRoles = await _unitOfWork.Roles.GetAllAsync(cancellationToken);
        var gerenteRole = allRoles.FirstOrDefault(r => r.Name.Equals("Gerente", StringComparison.OrdinalIgnoreCase));

        if (gerenteRole == null)
            return new List<Domain.Entities.Users.User>();

        // Get all users with Gerente role
        var allUsers = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        return allUsers.Where(u => u.RoleId == gerenteRole.Id).ToList();
    }
}
