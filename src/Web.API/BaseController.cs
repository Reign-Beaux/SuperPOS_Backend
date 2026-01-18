using Application.DesignPatterns.OperationResults;

namespace Web.API;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(OperationResult<T> result, string? actionName = null)
    {
        return result.Status switch
        {
            StatusResult.Ok
                => Ok(result.Value),

            StatusResult.NoContent
                => NoContent(),

            StatusResult.Created
                when actionName is not null && result.Value is not null
                => CreatedAtAction(actionName, new { id = result.Value }, result.Value),

            StatusResult.Exists
                => Conflict(result.Error),

            StatusResult.Conflict
                => Conflict(result.Error),

            StatusResult.BadRequest
                => BadRequest(result.Error),

            StatusResult.NotFound
                => NotFound(result.Error!.Detail),

            _ => StatusCode((int)result.Status, result.Error!.Detail ?? "Unexpected error")
        };
    }
}