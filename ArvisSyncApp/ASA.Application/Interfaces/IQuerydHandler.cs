namespace ASA.Application.Interfaces;

public interface IQuerydHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery command, CancellationToken cancellationToken);
}