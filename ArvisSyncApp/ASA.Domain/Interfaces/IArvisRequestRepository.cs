namespace ASA.Domain.Interfaces;

public interface IArvisRequestRepository
{
    Task<ArvisRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<long> SaveAsync(ArvisRequest request, CancellationToken cancellationToken = default);
}