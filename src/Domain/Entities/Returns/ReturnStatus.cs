namespace Domain.Entities.Returns;

/// <summary>
/// Status of a return request.
/// </summary>
public enum ReturnStatus
{
    /// <summary>
    /// Return is pending approval
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Return has been approved and processed
    /// </summary>
    Approved = 2,

    /// <summary>
    /// Return has been rejected
    /// </summary>
    Rejected = 3
}
