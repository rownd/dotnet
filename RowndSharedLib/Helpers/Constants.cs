using System.Diagnostics;
using System.Reflection;

namespace Rownd.Helpers;

public static class RowndConstants
{
    public static FileVersionInfo SdkVersion = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(RowndClient)).Location);
    public static string DefaultUserAgent = $"Rownd SDK for .NET/{SdkVersion.FileVersion} (Language: C#; Platform: {Environment.OSVersion.VersionString})";

}