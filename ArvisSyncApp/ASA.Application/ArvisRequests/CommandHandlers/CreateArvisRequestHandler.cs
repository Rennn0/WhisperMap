using ASA.Application.ArvisRequests.Commands;
using ASA.Application.Interfaces;
using ASA.Domain;
using ASA.Domain.Interfaces;

namespace ASA.Application.ArvisRequests.CommandHandlers;

public sealed class CreateArvisRequestHandler : ICommandHandler<CreateArvisRequest, long>
{
    private readonly IEventRepository<ArvisRequest> _repository;

    public CreateArvisRequestHandler(IEventRepository<ArvisRequest> repository) =>
        _repository = repository;

    public Task<long> HandleAsync(CreateArvisRequest command, CancellationToken cancellationToken)
    {
        ArvisRequest ar = ArvisRequest.Create(command.Id, command.Title, command.Headers, command.Cookies,
            DateTimeOffset.Now);
        return _repository.SaveAsync(ar, cancellationToken);
    }
}