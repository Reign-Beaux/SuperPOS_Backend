namespace Application.UseCases.Inventories;

/// <summary>
/// Defines the types of inventory operations that can be performed.
/// </summary>
public enum InventoryOperation
{
    /// <summary>
    /// Add quantity to existing stock.
    /// </summary>
    Add = 0,

    /// <summary>
    /// Set stock to a specific quantity (absolute value).
    /// </summary>
    Set = 1,

    /// <summary>
    /// Remove quantity from existing stock.
    /// </summary>
    Remove = 2
}
