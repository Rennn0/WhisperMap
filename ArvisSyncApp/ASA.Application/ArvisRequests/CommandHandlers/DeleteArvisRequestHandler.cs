using ASA.Application.ArvisRequests.Commands;
using ASA.Application.Interfaces;
using ASA.Domain;
using ASA.Domain.Exceptions;
using ASA.Domain.Interfaces;

namespace ASA.Application.ArvisRequests.CommandHandlers;

public sealed class DeleteArvisRequestHandler : ICommandHandler<DeleteArvisRequest>
{
    private readonly IEventRepository<ArvisRequest> _repository;

    public DeleteArvisRequestHandler(IEventRepository<ArvisRequest> repository) =>
        _repository = repository;

    public async Task HandleAsync(DeleteArvisRequest command, CancellationToken cancellationToken)
    {
        ArvisRequest ar = await _repository.GetAsync(command.Id, cancellationToken) ??
                          throw new Exception(nameof(ArvisRequest));
        if (command.ExpectedVersion.HasValue && command.ExpectedVersion.Value != ar.Version)
            throw new ConcurrencyException(command.Id, command.ExpectedVersion.Value, ar.Version);

        ar.Delete(DateTimeOffset.Now);
        await _repository.SaveAsync(ar, cancellationToken);
    }
}