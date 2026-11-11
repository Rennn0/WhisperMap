using ASA.Application.Interfaces;
using ASA.Domain;
using ASA.Domain.Enums;

namespace ASA.Application.PopularOptions.Queries;

public sealed record ListPopularOptions(ConfigPlace? ConfigPlace) : IQuery<IReadOnlyList<PopularRequestOption>>;