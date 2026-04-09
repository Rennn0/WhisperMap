using Microsoft.Extensions.Configuration;

namespace XcLib.Sse.Configuration;

public class DefaultSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new DefaultProvider();
}