using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DCRManagement.UI.Common;

/// <summary>
/// Persists multiple login credentials securely using Windows DPAPI.
/// Stored at %AppData%\DCRManagement\credentials.dat — encrypted, never plain text.
/// </summary>
public static class CredentialStore
{
    private static readonly string _filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DCRManagement",
        "credentials.dat");

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Save or update a credential entry.
    /// If the username already exists, its password is updated.
    /// The most recently used account is moved to the top of the list.
    /// </summary>
    public static void Save(string username, string password)
    {
        try
        {
            var list = LoadAll().ToList();

            // Remove existing entry for this username (case-insensitive)
            list.RemoveAll(c => c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            // Insert at top — most recently used first
            list.Insert(0, new SavedCredential(username, password));

            Persist(list);
        }
        catch { /* non-critical */ }
    }

    /// <summary>Returns all saved credentials, most recently used first.</summary>
    public static IReadOnlyList<SavedCredential> LoadAll()
    {
        try
        {
            if (!File.Exists(_filePath)) return [];

            var encrypted  = File.ReadAllBytes(_filePath);
            var plainBytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var json       = Encoding.UTF8.GetString(plainBytes);

            return JsonSerializer.Deserialize<List<SavedCredential>>(json) ?? [];
        }
        catch { return []; }
    }

    /// <summary>Remove a single saved credential by username.</summary>
    public static void Remove(string username)
    {
        try
        {
            var list = LoadAll().ToList();
            list.RemoveAll(c => c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            Persist(list);
        }
        catch { }
    }

    /// <summary>Delete all saved credentials.</summary>
    public static void ClearAll()
    {
        try { if (File.Exists(_filePath)) File.Delete(_filePath); }
        catch { }
    }

    /// <summary>True if at least one credential is saved.</summary>
    public static bool HasAny => File.Exists(_filePath) && LoadAll().Count > 0;

    // ── Private ───────────────────────────────────────────────────────────────

    private static void Persist(IEnumerable<SavedCredential> list)
    {
        var json       = JsonSerializer.Serialize(list.ToList());
        var plainBytes = Encoding.UTF8.GetBytes(json);
        var encrypted  = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);

        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        File.WriteAllBytes(_filePath, encrypted);
    }
}

/// <summary>A single saved credential entry.</summary>
public record SavedCredential(string Username, string Password);
