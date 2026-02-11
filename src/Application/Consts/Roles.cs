namespace Application.Consts;

/// <summary>
/// Constants for role names used in authorization policies.
/// Role names must match exactly with database values.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role with full system access.
    /// </summary>
    public const string Admin = "Administrador";

    /// <summary>
    /// Manager role with sales and inventory management access.
    /// </summary>
    public const string Manager = "Gerente";

    /// <summary>
    /// Seller role with basic sales and query access.
    /// </summary>
    public const string Seller = "Vendedor";

    // Combined roles for convenience
    public const string AdminOrManager = $"{Admin},{Manager}";
    public const string AdminOrManagerOrSeller = $"{Admin},{Manager},{Seller}";
}
