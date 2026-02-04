namespace Application.UseCases.Inventories;

/// <summary>
/// Defines the types of inventory operations that can be performed.
/// </summary>
public enum InventoryOperation
{
    /// <summary>
    /// Add quantity to existing stock.
    /// </summary>
    Add,

    /// <summary>
    /// Set stock to a specific quantity (absolute value).
    /// </summary>
    Set
}
