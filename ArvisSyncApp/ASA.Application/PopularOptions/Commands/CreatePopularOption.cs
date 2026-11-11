using ASA.Application.Interfaces;
using ASA.Domain;

namespace ASA.Application.PopularOptions.Commands;

public record CreatePopularOption(PopularRequestOption Option) : ICommand<long>;