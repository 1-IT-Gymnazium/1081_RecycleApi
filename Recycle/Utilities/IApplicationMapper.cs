using Microsoft.Extensions.Options;
using NodaTime;
using Recycle.Api.Settings;

namespace Recycle.Api.Utilities;

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
