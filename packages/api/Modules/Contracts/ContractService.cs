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

        var contract = await _contractRepository.LoadWithTasksAsync(id, cancellationToken);
        if (contract is null) return null;

        contract.CustomerId = request.CustomerId;
        contract.ConsultantId = request.ConsultantId;
        contract.Subject = request.Subject.Trim();
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;

        var requestedIds = request.Tasks
            .Where(taskRequest => taskRequest.Id.HasValue)
            .Select(taskRequest => taskRequest.Id!.Value)
            .ToHashSet();

        var existingTaskIds = contract.Tasks.Select(task => task.Id).ToHashSet();
        if (requestedIds.Except(existingTaskIds).Any())
            throw new InvalidContractRequestException("One or more task IDs do not belong to this contract.");

        foreach (var existingTask in contract.Tasks.Where(task => !task.IsArchived))
        {
            if (!requestedIds.Contains(existingTask.Id))
                existingTask.IsArchived = true;
        }

        var nextOrder = contract.Tasks.Count > 0 ? contract.Tasks.Max(task => task.Order) + 1 : 0;

        foreach (var taskRequest in request.Tasks)
        {
            if (taskRequest.Id.HasValue)
            {
                var existingTask = contract.Tasks.FirstOrDefault(task => task.Id == taskRequest.Id.Value);
                if (existingTask is not null)
                {
                    existingTask.Name = taskRequest.Name.Trim();
                    existingTask.DayRate = taskRequest.DayRate;
                    existingTask.IsArchived = false;
                }
            }
            else
            {
                _contractRepository.AddTask(BuildTask(taskRequest, nextOrder++, id));
            }
        }

        await _contractRepository.SaveAsync(cancellationToken);

        var updated = await _contractRepository.GetByIdAsync(id, cancellationToken);
        return updated is null ? null : ToResponse(updated);
    }

    private static ContractTask BuildTask(ContractTaskRequest taskRequest, int order, Guid contractId) =>
        new()
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            Name = taskRequest.Name.Trim(),
            DayRate = taskRequest.DayRate,
            Order = order,
        };

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
