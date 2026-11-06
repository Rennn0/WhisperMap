namespace ASA.Domain.Interfaces;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
    TResult Handle(TCommand command);
}