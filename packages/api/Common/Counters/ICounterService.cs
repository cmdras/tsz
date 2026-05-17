namespace Api.Common.Counters;

public interface ICounterService
{
    Task<int> NextAsync(string key, CancellationToken cancellationToken = default);
}
