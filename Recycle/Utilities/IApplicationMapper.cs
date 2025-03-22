using Microsoft.Extensions.Options;
using NodaTime;
using Recycle.Api.Settings;

namespace Recycle.Api.Utilities;

/// <summary>
/// Provides access to system-wide utilities like current time and environment configuration.
/// </summary>
public interface IApplicationMapper
{
    public Instant Now { get; }
    public EnviromentSettings EnviromentSettings { get; }
}

public class ApplicationMapper(IClock Clock, IOptions<EnviromentSettings> environmentSettings) : IApplicationMapper
{
    public Instant Now => Clock.GetCurrentInstant();
    public EnviromentSettings EnviromentSettings => environmentSettings.Value;
}
