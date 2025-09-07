using System.Reflection;

namespace OnlineEventTicketing.Helpers
{
    public static class VersionHelper
    {
        public static string GetVersion()
        {
            try
            {
                var fullVersion = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion ?? "1.0.0";
                
                var cleanVersion = fullVersion.Split('+')[0];
                return string.IsNullOrEmpty(cleanVersion) ? "1.0.0" : cleanVersion;
            }
            catch
            {
                return "1.0.0";
            }
        }
    }
}