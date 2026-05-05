using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DCRManagement.UI.Common;

/// <summary>
/// Persists login credentials securely using Windows DPAPI (ProtectedData).
/// The encrypted blob is stored in %AppData%\DCRManagement\credentials.dat.
/// Only the Windows user account that encrypted the data can decrypt it.
/// Plain-text passwords are NEVER written to disk.
/// </summary>
public static class CredentialStore
{
    private static readonly string _filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DCRManagement",
        "credentials.dat");

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Save username + password encrypted with DPAPI.</summary>
    public static void Save(string username, string password)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { username, password });
            var plainBytes = Encoding.UTF8.GetBytes(payload);

            // Encrypt — only decryptable by the same Windows user on the same machine
            var encrypted = ProtectedData.Protect(
                plainBytes,
                null,
                DataProtectionScope.CurrentUser);

            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllBytes(_filePath, encrypted);
        }
        catch
        {
            // Non-critical — silently ignore save failures
        }
    }

    /// <summary>Load saved credentials. Returns null if none saved or decryption fails.</summary>
    public static (string Username, string Password)? Load()
    {
        try
        {
            if (!File.Exists(_filePath)) return null;

            var encrypted = File.ReadAllBytes(_filePath);
            var plainBytes = ProtectedData.Unprotect(
                encrypted,
                null,
                DataProtectionScope.CurrentUser);

            var json = Encoding.UTF8.GetString(plainBytes);
            var doc  = JsonSerializer.Deserialize<JsonElement>(json);

            var username = doc.GetProperty("username").GetString() ?? string.Empty;
            var password = doc.GetProperty("password").GetString() ?? string.Empty;

            return (username, password);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Delete saved credentials.</summary>
    public static void Clear()
    {
        try { if (File.Exists(_filePath)) File.Delete(_filePath); }
        catch { /* ignore */ }
    }

    /// <summary>True if a saved credential file exists.</summary>
    public static bool HasSaved => File.Exists(_filePath);
}
