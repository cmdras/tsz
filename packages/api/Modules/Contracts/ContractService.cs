using Api.Common;
using Api.Common.Counters;
using Api.Common.Database;
using Api.Common.Exceptions;
using Api.Modules.Users;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Contracts;

public class InvalidContractRequestException(string message) : DomainException(message, 422);

public class ContractService
{
    private readonly AppDbContext _dbContext;
    private readonly ICounterService _counterService;

    public ContractService(AppDbContext dbContext, ICounterService counterService)
    {
        _dbContext = dbContext;
        _counterService = counterService;
    }

    public async Task<PagedContracts> GetAllAsync(
        string? search,
        ContractSort sort,
        SortDirection sortDirection,
        int page,
        int pageSize,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Contract> query = _dbContext.Contracts
            .Include(contract => contract.Customer)
            .Include(contract => contract.Consultant);

        if (!includeArchived)
            query = query.Where(contract => !contract.IsArchived);


        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(contract =>
                contract.Subject.ToLower().Contains(term) ||
                contract.Customer.Name.ToLower().Contains(term) ||
                contract.Consultant.Name.ToLower().Contains(term));
        }

        var isDescending = sortDirection == SortDirection.Desc;
        query = (sort, isDescending) switch
        {
            (ContractSort.Customer, true) => query.OrderByDescending(contract => contract.Customer.Name),
            (ContractSort.Customer, false) => query.OrderBy(contract => contract.Customer.Name),
            (ContractSort.Subject, true) => query.OrderByDescending(contract => contract.Subject),
            (ContractSort.Subject, false) => query.OrderBy(contract => contract.Subject),
            (ContractSort.Consultant, true) => query.OrderByDescending(contract => contract.Consultant.Name),
            (ContractSort.Consultant, false) => query.OrderBy(contract => contract.Consultant.Name),
            (ContractSort.StartDate, true) => query.OrderByDescending(contract => contract.StartDate),
            (ContractSort.StartDate, false) => query.OrderBy(contract => contract.StartDate),
            (ContractSort.EndDate, true) => query.OrderByDescending(contract => contract.EndDate),
            (ContractSort.EndDate, false) => query.OrderBy(contract => contract.EndDate),
            (_, true) => query.OrderByDescending(contract => contract.Number),
            (_, false) => query.OrderBy(contract => contract.Number),
        };

        var total = await query.CountAsync(cancellationToken);
        var entities = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedContracts(entities.Select(ToResponse).ToList(), total);
    }

    public async Task<ContractResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _dbContext.Contracts
            .Include(loaded => loaded.Customer)
            .Include(loaded => loaded.Consultant)
            .Include(loaded => loaded.Tasks)
            .FirstOrDefaultAsync(loaded => loaded.Id == id, cancellationToken);
        return contract is null ? null : ToResponse(contract);
    }

    public async Task<ContractResponse> CreateAsync(ContractRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(request, cancellationToken);

        var number = await _counterService.NextAsync(CounterKeys.Contract, cancellationToken);

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            Number = number,
            CustomerId = request.CustomerId,
            ConsultantId = request.ConsultantId,
            Subject = request.Subject.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
        };

        var order = 0;
        foreach (var taskRequest in request.Tasks)
            contract.Tasks.Add(BuildTask(taskRequest, order++));

        await _dbContext.Contracts.AddAsync(contract, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await LoadReferencesAsync(contract, cancellationToken);

        return ToResponse(contract);
    }

    public async Task<ContractResponse?> UpdateAsync(Guid id, ContractRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(request, cancellationToken);

        var contract = await _dbContext.Contracts
            .Include(loaded => loaded.Tasks)
            .FirstOrDefaultAsync(loaded => loaded.Id == id, cancellationToken);

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
                _dbContext.ContractTasks.Add(BuildTask(taskRequest, nextOrder++, id));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await LoadReferencesAsync(contract, cancellationToken);

        return ToResponse(contract);
    }

    private static ContractTask BuildTask(ContractTaskRequest taskRequest, int order, Guid contractId = default) =>
        new()
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            Name = taskRequest.Name.Trim(),
            DayRate = taskRequest.DayRate,
            Order = order,
        };

    private async Task LoadReferencesAsync(Contract contract, CancellationToken cancellationToken)
    {
        await _dbContext.Entry(contract).Reference(loaded => loaded.Customer).LoadAsync(cancellationToken);
        await _dbContext.Entry(contract).Reference(loaded => loaded.Consultant).LoadAsync(cancellationToken);
        await _dbContext.Entry(contract).Collection(loaded => loaded.Tasks).LoadAsync(cancellationToken);
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

    public async Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _dbContext.Contracts.FindAsync([id], cancellationToken);
        if (contract is null) return false;

        contract.IsArchived = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contract = await _dbContext.Contracts.FindAsync([id], cancellationToken);
        if (contract is null) return false;

        contract.IsArchived = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateRequestAsync(ContractRequest request, CancellationToken cancellationToken)
    {
        if (request.EndDate.HasValue && request.StartDate > request.EndDate.Value)
            throw new InvalidContractRequestException("StartDate must be on or before EndDate.");

        if (request.Tasks.Count == 0)
            throw new InvalidContractRequestException("At least one task is required.");

        var customer = await _dbContext.Customers.FindAsync([request.CustomerId], cancellationToken);
        if (customer is null || customer.IsArchived)
            throw new InvalidContractRequestException("Customer must be a non-archived customer.");

        var consultant = await _dbContext.Users.FindAsync([request.ConsultantId], cancellationToken);
        if (consultant is null || consultant.IsArchived || consultant.Role == UserRole.ClientManager)
            throw new InvalidContractRequestException("Consultant must be a non-archived user with a role other than ClientManager.");
    }
}
