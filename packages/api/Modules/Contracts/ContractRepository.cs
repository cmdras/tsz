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

    public async Task<Contract?> LoadWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Contracts
            .Include(contract => contract.Tasks)
            .FirstOrDefaultAsync(contract => contract.Id == id, cancellationToken);
    }

    public void AddTask(ContractTask task)
    {
        _dbContext.ContractTasks.Add(task);
    }

    public Task SaveAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => SetArchivedAsync(id, true, cancellationToken);

    public Task<bool> UnarchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => SetArchivedAsync(id, false, cancellationToken);

    private async Task<bool> SetArchivedAsync(Guid id, bool isArchived, CancellationToken cancellationToken)
    {
        var contract = await _dbContext.Contracts.FindAsync([id], cancellationToken);
        if (contract is null) return false;

        contract.IsArchived = isArchived;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
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
}
