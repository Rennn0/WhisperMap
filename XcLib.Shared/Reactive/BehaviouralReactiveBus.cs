using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

namespace XcLib.Shared.Reactive;

public class BehaviouralReactiveBus<T> : DefaultReactiveBus<T> where T : class
{
    public BehaviouralReactiveBus(ILogger<T> logger) : base(logger) => Subject = new BehaviorSubject<T>(null!);
}