using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Repositories;
using Application.UseCases.Users.DTOs;

namespace Application.UseCases.Users.CQRS.Queries.Search;

public sealed class UserSearchHandler : IRequestHandler<UserSearchQuery, OperationResult<IEnumerable<UserDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public UserSearchHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<IEnumerable<UserDTO>>> Handle(
        UserSearchQuery request,
        CancellationToken cancellationToken)
    {
        // Validate minimum search term length
        if (string.IsNullOrWhiteSpace(request.Term) || request.Term.Length < 3)
        {
            return Result.Error(
                ErrorResult.BadRequest,
                detail: "El término de búsqueda debe tener al menos 3 caracteres.");
        }

        var users = await _unitOfWork.Users.SearchByNameAsync(request.Term, cancellationToken);

        var usersDto = _mapper.Map<IEnumerable<UserDTO>>(users);

        return Result.Success(usersDto);
    }
}
