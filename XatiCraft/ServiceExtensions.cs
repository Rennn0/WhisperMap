using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using XatiCraft.Data.Repos.EfCoreImpl;

namespace XatiCraft;

/// <summary>
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void AddCert(this IServiceCollection services)
    {
        services.AddDataProtection()
            .PersistKeysToDbContext<ApplicationContext>()
            .ProtectKeysWithCertificate(new X509Certificate2(
                Environment.GetEnvironmentVariable("CERT_PATH") ?? throw new InvalidOperationException(),
                Environment.GetEnvironmentVariable("CERT_PASS") ?? throw new InvalidOperationException()))
            .SetApplicationName("XatiCraft");
    }
}