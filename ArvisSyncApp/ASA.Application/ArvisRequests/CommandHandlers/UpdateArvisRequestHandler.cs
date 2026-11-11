using ASA.Application.ArvisRequests.Commands;
using ASA.Application.Interfaces;
using ASA.Domain;
using ASA.Domain.Exceptions;
using ASA.Domain.Interfaces;
using Polly;
using Polly.Retry;

namespace ASA.Application.ArvisRequests.CommandHandlers;

public sealed class UpdateArvisRequestHandler : ICommandHandler<UpdateArvisRequest, long>
{
    private static readonly ResiliencePipeline ConcurrencyRetry = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<ConcurrencyException>(),
            MaxRetryAttempts = 2,
            Delay = TimeSpan.FromMilliseconds(10),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        }).Build();

    private readonly IEventRepository<ArvisRequest> _repository;

    public UpdateArvisRequestHandler(IEventRepository<ArvisRequest> repository) =>
        _repository = repository;

    public async Task<long> HandleAsync(UpdateArvisRequest command, CancellationToken cancellationToken)
    {
        if (command.ExpectedVersion.HasValue)
            return await CoreHandleAsync(command, cancellationToken);
        return await ConcurrencyRetry.ExecuteAsync(
            static (state, ct) => new ValueTask<long>(state.Handler.CoreHandleAsync(state.Command, ct)),
            (Handler: this, Command: command),
            cancellationToken);
    }

    private async Task<long> CoreHandleAsync(UpdateArvisRequest command, CancellationToken cancellationToken)
    {
        ArvisRequest ar = await _repository.GetAsync(command.Id, cancellationToken) is
            { IsDeleted: false } found
            ? found
            : throw new InvalidDataException();
        if (command.ExpectedVersion.HasValue && command.ExpectedVersion != ar.Version)
            throw new ConcurrencyException(command.Id, command.ExpectedVersion.Value, ar.Version);
        DateTimeOffset time = DateTimeOffset.Now;
        if (command.Title is not null)
            ar.Rename(command.Title, time);
        if (command.Headers is not null)
            ar.ReplaceHeaders(command.Headers, time);
        if (command.Cookies is not null)
            ar.ReplaceCookies(command.Cookies, time);
        return await _repository.SaveAsync(ar, cancellationToken);
    }
}