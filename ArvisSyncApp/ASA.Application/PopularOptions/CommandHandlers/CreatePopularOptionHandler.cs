using ASA.Application.Interfaces;
using ASA.Application.PopularOptions.Commands;
using ASA.Domain;
using ASA.Domain.Interfaces;

namespace ASA.Application.PopularOptions.CommandHandlers;

public sealed class CreatePopularOptionHandler : ICommandHandler<CreatePopularOption, long>
{
    private readonly IRepository<PopularRequestOption> _repository;

    public CreatePopularOptionHandler(IRepository<PopularRequestOption> repository) => _repository = repository;

    public async Task<long> HandleAsync(CreatePopularOption command, CancellationToken cancellationToken)
    {
        command.Option.Id = 0;
        return await _repository.SaveAsync(command.Option, cancellationToken);
    }
}