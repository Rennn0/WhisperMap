using ASA.Domain.Abstractions;
using ASA.Domain.Events;
using ASA.Domain.Exceptions;
using ASA.Domain.Interfaces;
using ASA.Domain.ValueObjects;

namespace ASA.Domain;

public sealed class ArvisRequest : AggregateRoot
{
    public const int MaxTitleLength = 128;

    public bool IsDeleted { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public IReadOnlyList<KeyValueProperty> Headers => _headers.AsReadOnly();
    public IReadOnlyList<KeyValueProperty> Cookiess => _cookies.AsReadOnly();

    private readonly List<KeyValueProperty> _headers = [];
    private readonly List<KeyValueProperty> _cookies = [];

    private ArvisRequest()
    {
    }

    public static ArvisRequest Create(Guid id, string title, IReadOnlyList<KeyValueProperty> headers,
        IReadOnlyList<KeyValueProperty> cookies, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(cookies);

        ArvisRequest ar = new ArvisRequest();
        ar.Raise(new ArvisRequestCreated(id, now, ValidTitle(title), [.. headers], [.. cookies]));
        return ar;
    }

    public static ArvisRequest Evolve(ArvisRequest? state, IDomainEvent @event, long version)
    {
        state ??= new ArvisRequest();
        state.ApplyPersisted(@event, version);
        return state;
    }

    public void Rename(string title, DateTimeOffset time)
    {
        EnsureNotDeleted();
        title = ValidTitle(title);
        if (title == Title) return;
        Raise(new ArvisRequestRenamed(Id, time, title));
    }

    public void ReplaceHeaders(IReadOnlyList<KeyValueProperty> headers, DateTimeOffset time)
    {
        ArgumentNullException.ThrowIfNull(headers);
        EnsureNotDeleted();
        if (headers.SequenceEqual(_headers)) return;
        Raise(new ArvisRequestHeaderReplaced(Id, time, [.. headers]));
    }

    public void ReplaceCookies(IReadOnlyList<KeyValueProperty> cookies, DateTimeOffset time)
    {
        ArgumentNullException.ThrowIfNull(cookies);
        EnsureNotDeleted();
        if (cookies.SequenceEqual(_cookies)) return;
        Raise(new ArvisRequestCookieReplaced(Id, time, [.. cookies]));
    }

    public void Delete(DateTimeOffset time)
    {
        if (IsDeleted) return;
        Raise(new ArvisRequestDeleted(Id, time));
    }

    protected override void ApplyCore(IDomainEvent @event)
    {
        switch (@event)
        {
            case ArvisRequestCreated e:
                Id = e.AggregateId;
                Title = e.Title;
                _headers.AddRange(e.Headers);
                _cookies.AddRange(e.Cookies);
                break;
            case ArvisRequestRenamed e:
                Title = e.NewTitle; break;
            case ArvisRequestHeaderReplaced e:
                _headers.Clear();
                _headers.AddRange(e.Headers);
                break;
            case ArvisRequestCookieReplaced e:
                _cookies.Clear();
                _cookies.AddRange(e.Cookies);
                break;
            case ArvisRequestDeleted: IsDeleted = true; break;
            default: throw new ArgumentOutOfRangeException(nameof(@event), @event.GetType().Name);
        }
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted) throw new DomainException(nameof(EnsureNotDeleted));
    }

    private static string ValidTitle(string? title)
    {
        title = title?.Trim() ?? string.Empty;
        return title.Length > MaxTitleLength ? throw new DomainException(nameof(ValidTitle)) : title;
    }
}