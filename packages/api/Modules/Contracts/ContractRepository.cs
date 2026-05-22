using System.Data;
using Api.Common;
using Api.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Contracts;

public class ContractRepository : IContractRepository
{
    private readonly AppDbContext _dbContext;

    public ContractRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<Contract> Items, int Total)> GetAllAsync(
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
            (ContractSort.Customer, _) => query.OrderBy(contract => contract.Customer.Name),
            (ContractSort.Subject, true) => query.OrderByDescending(contract => contract.Subject),
            (ContractSort.Subject, _) => query.OrderBy(contract => contract.Subject),
            (ContractSort.Consultant, true) => query.OrderByDescending(contract => contract.Consultant.Name),
            (ContractSort.Consultant, _) => query.OrderBy(contract => contract.Consultant.Name),
            (ContractSort.StartDate, true) => query.OrderByDescending(contract => contract.StartDate),
            (ContractSort.StartDate, _) => query.OrderBy(contract => contract.StartDate),
            (ContractSort.EndDate, true) => query.OrderByDescending(contract => contract.EndDate),
            (ContractSort.EndDate, _) => query.OrderBy(contract => contract.EndDate),
            (_, true) => query.OrderByDescending(contract => contract.Number),
            _ => query.OrderBy(contract => contract.Number),
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Contracts
            .Include(contract => contract.Customer)
            .Include(contract => contract.Consultant)
            .Include(contract => contract.Tasks)
            .FirstOrDefaultAsync(contract => contract.Id == id, cancellationToken);
    }

    public async Task<Contract> CreateAsync(ContractRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var nextNumber = (await _dbContext.Contracts.MaxAsync(contract => (int?)contract.Number, cancellationToken) ?? 0) + 1;

        var contractId = Guid.NewGuid();
        var contract = new Contract
        {
            Id = contractId,
            Number = nextNumber,
            CustomerId = request.CustomerId,
            ConsultantId = request.ConsultantId,
            Subject = request.Subject.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Tasks = request.Tasks.Select((taskRequest, index) => BuildTask(taskRequest, index, contractId)).ToList(),
        };

        _dbContext.Contracts.Add(contract);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return contract;
    }

    public async Task<Contract?> UpdateAsync(Guid id, ContractRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await _dbContext.Contracts
            .Include(entity => entity.Tasks)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
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

        return await _dbContext.Contracts
            .Include(entity => entity.Customer)
            .Include(entity => entity.Consultant)
            .Include(entity => entity.Tasks)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.SetArchivedAsync<Contract>(id, true, cancellationToken);

    public Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.SetArchivedAsync<Contract>(id, false, cancellationToken);

    private static ContractTask BuildTask(ContractTaskRequest taskRequest, int order, Guid contractId) =>
        new()
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            Name = taskRequest.Name.Trim(),
            DayRate = taskRequest.DayRate,
            Order = order,
        };
}
