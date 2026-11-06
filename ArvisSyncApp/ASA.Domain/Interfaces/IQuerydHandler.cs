namespace ASA.Domain.Interfaces;

public interface IQuerydHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery command, CancellationToken cancellationToken);
    TResult Handle(TQuery command);
}