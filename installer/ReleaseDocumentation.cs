namespace BopItAccess.Installer;

// Source archives retain contributor documentation. Compiled installations
// contain the user guide and legal notices only; never copy the source tree
// wholesale into the player's game directory.
internal static class ReleaseDocumentation
{
    internal const string ModLicenseName = "BopItAccess-LICENSE.txt";
    internal static readonly string[] LocaleFolders =
        { "de", "es", "es-MX", "fr", "it", "ja", "ko", "pt-BR", "zh" };

    private static readonly HashSet<string> PlayerNames = new(StringComparer.OrdinalIgnoreCase)
        { "BopItAccess-user-guide.html", "THIRD-PARTY-NOTICES.txt" };
    private static readonly HashSet<string> DevelopmentNames = new(StringComparer.OrdinalIgnoreCase)
        { "BopItAccess-build-history.html", "BopItAccess-release-review.html",
          "GIT-WORKFLOW.md", "README.md", "README.txt" };

    internal static bool IsPackageFile(string relativePath)
    {
        string[] parts = relativePath.Replace('\\', '/').Split('/');
        if (parts.Any(part => part.Length == 0 || part is "." or "..")) return false;
        if (DevelopmentNames.Contains(parts[^1])) return false;
        if (parts.Length == 1)
            return PlayerNames.Contains(parts[0]) ||
                parts[0].Equals(ModLicenseName, StringComparison.OrdinalIgnoreCase);
        if (parts.Length == 2 && LocaleFolders.Contains(parts[0], StringComparer.OrdinalIgnoreCase))
            return PlayerNames.Contains(parts[1]);
        // Official Prism licence files live separately from project documents.
        return parts.Length >= 3 && parts[0].Equals("THIRD-PARTY-LICENSES", StringComparison.OrdinalIgnoreCase) &&
            parts[1].Equals("Prism", StringComparison.OrdinalIgnoreCase) &&
            ((parts.Length == 3 && parts[2].Equals("NOTICE", StringComparison.OrdinalIgnoreCase)) ||
             (parts.Length >= 4 && parts[2].Equals("LICENSES", StringComparison.OrdinalIgnoreCase)));
    }

    internal static bool IsRetiredInstalledPath(string documentationRoot, string file)
    {
        string relative = Path.GetRelativePath(documentationRoot, file);
        string[] parts = relative.Replace('\\', '/').Split('/');
        return (parts.Length == 1 ||
                (parts.Length == 2 && LocaleFolders.Contains(parts[0], StringComparer.OrdinalIgnoreCase))) &&
            DevelopmentNames.Contains(parts[^1]);
    }
}
