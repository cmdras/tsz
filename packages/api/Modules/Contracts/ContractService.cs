using Api.Common;
using Api.Common.Exceptions;
using Api.Modules.Customers;
using Api.Modules.Users;

namespace Api.Modules.Contracts;

public class InvalidContractRequestException(string message) : DomainException(message, 422);

public class ContractService(IContractRepository contractRepository, ICustomerRepository customerRepository, IUserRepository userRepository)
{
    private readonly IContractRepository _contractRepository = contractRepository;
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<PagedContracts> GetAllAsync(
        string? search,
        ContractSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        var (entities, total) = await _contractRepository.GetAllAsync(search, sort, sortDirection, page, pageSize, includeArchived, cancellationToken);
        return new PagedContracts(entities.Select(ToResponse).ToList(), total);
    }

    public async Task<ContractResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdAsync(id, cancellationToken);
        return contract is null ? null : ToResponse(contract);
    }

    public async Task<ContractResponse> CreateAsync(ContractRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(request, cancellationToken);

        var contract = await _contractRepository.CreateAsync(request, cancellationToken);

        var loaded = await _contractRepository.GetByIdAsync(contract.Id, cancellationToken);
        return ToResponse(loaded!);
    }

    public async Task<ContractResponse?> UpdateAsync(Guid id, ContractRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(request, cancellationToken);

        var contract = await _contractRepository.UpdateAsync(id, request, cancellationToken);
        return contract is null ? null : ToResponse(contract);
    }

    private static ContractResponse ToResponse(Contract contract) => new(
        contract.Id,
        contract.Number,
        contract.CustomerId,
        contract.Customer.Name,
        contract.ConsultantId,
        contract.Consultant.Name,
        contract.Subject,
        contract.StartDate,
        contract.EndDate,
        contract.IsArchived,
        contract.Tasks
            .OrderBy(task => task.Order)
            .Select(task => new ContractTaskResponse(task.Id, task.Name, task.DayRate, task.Order, task.IsArchived))
            .ToList());

    public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _contractRepository.ArchiveAsync(id, cancellationToken);

    public Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _contractRepository.UnarchiveAsync(id, cancellationToken);

    private async Task ValidateRequestAsync(ContractRequest request, CancellationToken cancellationToken)
    {
        if (request.EndDate.HasValue && request.StartDate > request.EndDate.Value)
            throw new InvalidContractRequestException("StartDate must be on or before EndDate.");

        if (request.Tasks.Count == 0)
            throw new InvalidContractRequestException("At least one task is required.");

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null || customer.IsArchived)
            throw new InvalidContractRequestException("Customer must be a non-archived customer.");

        var consultant = await _userRepository.GetByIdAsync(request.ConsultantId, cancellationToken);
        if (consultant is null || consultant.IsArchived || consultant.Role == UserRole.ClientManager)
            throw new InvalidContractRequestException("Consultant must be a non-archived user with a role other than ClientManager.");
    }
}
