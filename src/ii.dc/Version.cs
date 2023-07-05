using ii.dc;
using System.Reflection;

[assembly: AssemblyFileVersion(VersionData.Version)]
[assembly: AssemblyInformationalVersion(VersionData.Version)]
[assembly: AssemblyProduct(VersionData.Version)]
[assembly: AssemblyVersion(VersionData.Version)]
[assembly: AssemblyTitle("title")]
[assembly: AssemblyCopyright("copy")]

namespace ii.dc
{
    public static class VersionData
    {
        // Note: This field is used to mark the version of the installer, which is compared against the RequiredInstallerVersion value in mod files
        // The value here is a string, but the comparison checks the major value (i.e. the first number) as an int
        public const string Version = "1.0.0.0";
    }
}