using ASA.Application.ArvisRequests.Commands;
using ASA.Application.Interfaces;
using ASA.Domain;
using ASA.Domain.Exceptions;
using ASA.Domain.Interfaces;

namespace ASA.Application.ArvisRequests.CommandHandlers;

public sealed class UpdateArvisRequestHandler : ICommandHandler<UpdateArvisRequest, long>
{
    private readonly IArvisRequestRepository _arvisRequestRepository;

    public UpdateArvisRequestHandler(IArvisRequestRepository arvisRequestRepository) =>
        _arvisRequestRepository = arvisRequestRepository;

    public async Task<long> HandleAsync(UpdateArvisRequest command, CancellationToken cancellationToken)
    {
        ArvisRequest? ar = await _arvisRequestRepository.GetAsync(command.Id, cancellationToken);
        if (ar is null or { IsDeleted: true }) throw new Exception(nameof(ArvisRequest));
        if (command.ExpectedVersion != ar.Version)
            throw new ConcurrencyException(command.Id, command.ExpectedVersion, ar.Version);
        DateTimeOffset time = DateTimeOffset.Now;
        ar.Rename(command.Title, time);
        ar.ReplaceHeaders(command.Headers, time);
        ar.ReplaceCookies(command.Cookies, time);
        return await _arvisRequestRepository.SaveAsync(ar, cancellationToken);
    }
}