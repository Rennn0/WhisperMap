using ASA.Application.Interfaces;
using ASA.Domain;

namespace ASA.Application.PopularOptions.Queries;

public sealed record GetPopularOption(long Id) : IQuery<PopularRequestOption?>;