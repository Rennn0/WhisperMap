using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

namespace XcLib.Shared.Reactive;

public class ReplayableReactiveBus<T> : DefaultReactiveBus<T> where T : class
{
    public ReplayableReactiveBus(ILogger<T> logger) : base(logger) => Subject = new ReplaySubject<T>(10);
}