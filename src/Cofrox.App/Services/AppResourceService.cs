using Cofrox.Core.Constants;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Cofrox.App.Services;

public sealed class AppResourceService
{
    private readonly ResourceLoader _resourceLoader = new();

    public string GetString(string key) =>
        (_resourceLoader.GetString(key) ?? string.Empty)
            .Replace("{BUILD_DATE}", BuildInfo.BuildDate, StringComparison.Ordinal);
}
