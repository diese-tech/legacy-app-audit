using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;
using SmitePnB.Models;

namespace SmitePnB.Services;

/// <summary>
/// Loads and caches everything from the Resources folder on disk:
/// god list, team configs, character images, and sound file paths.
///
/// The resources folder path comes from AppConfig and is set by the operator
/// in Settings. Every method here is fail-soft: missing images return null,
/// missing sound files return null — the app degrades gracefully rather than
/// crashing during a live broadcast.
/// </summary>
public class ResourceLoader
{
    private AppConfig _config = new();
    private readonly Dictionary<string, BitmapImage> _imageCache = [];
    private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AppConfig Config => _config;

    // ── Boot ──────────────────────────────────────────────────────────────

    public void Load(string configPath)
    {
        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            _config  = JsonSerializer.Deserialize<AppConfig>(json, _jsonOpts) ?? new AppConfig();
        }

        _imageCache.Clear();
    }

    public void SaveConfig(string configPath)
    {
        var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configPath, json);
    }

    // ── Validation ────────────────────────────────────────────────────────

    /// <summary>
    /// Checks that the resources path exists and has the expected sub-folders.
    /// Returns a list of problems — empty list means all clear.
    /// </summary>
    public List<string> VerifyResources()
    {
        var problems = new List<string>();
        if (string.IsNullOrWhiteSpace(_config.ResourcesPath) || !Directory.Exists(_config.ResourcesPath))
        {
            problems.Add($"Resources folder not found: '{_config.ResourcesPath}'");
            return problems;
        }

        foreach (var sub in new[] { "CharacterImages/Picks", "CharacterImages/Bans", "CharacterImages/TopBans", "Sounds", "Teams" })
        {
            if (!Directory.Exists(Path.Combine(_config.ResourcesPath, sub)))
                problems.Add($"Missing subfolder: {sub}");
        }

        if (!File.Exists(Path.Combine(_config.ResourcesPath, "CharactersList.txt")))
            problems.Add("Missing CharactersList.txt");

        return problems;
    }

    // ── Gods ──────────────────────────────────────────────────────────────

    public List<string> LoadGodList()
    {
        var path = Path.Combine(_config.ResourcesPath, "CharactersList.txt");
        if (!File.Exists(path)) return [];
        return File.ReadAllLines(path)
                   .Select(l => l.Trim())
                   .Where(l => l.Length > 0)
                   .ToList();
    }

    // ── Teams ─────────────────────────────────────────────────────────────

    /// <summary>Returns folder names under Resources/Teams/ sorted alphabetically.</summary>
    public List<string> GetTeamFolders()
    {
        var dir = Path.Combine(_config.ResourcesPath, "Teams");
        if (!Directory.Exists(dir)) return [];
        return Directory.GetDirectories(dir)
                        .Select(Path.GetFileName)
                        .Where(n => n != null)
                        .Select(n => n!)
                        .OrderBy(n => n)
                        .ToList();
    }

    public TeamConfig LoadTeam(string folderName)
    {
        var dir     = Path.Combine(_config.ResourcesPath, "Teams", folderName);
        var config  = new TeamConfig { FolderName = folderName };

        // Roster.txt — one player name per line, role order: Solo Jungle Mid Support Carry
        var rosterPath = Path.Combine(dir, "Roster.txt");
        if (File.Exists(rosterPath))
        {
            var lines = File.ReadAllLines(rosterPath)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 0)
                            .ToArray();
            for (int i = 0; i < Math.Min(lines.Length, config.Roster.Length); i++)
                config.Roster[i] = lines[i];
        }

        // BanData.json
        var banPath = Path.Combine(dir, "BanData.json");
        if (File.Exists(banPath))
        {
            try
            {
                var json = File.ReadAllText(banPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("teamname", out var tn))
                    config.TeamName = tn.GetString() ?? folderName;

                if (root.TryGetProperty("totalgames", out var tg))
                    config.TotalGames = tg.GetInt32();

                if (root.TryGetProperty("bancounts", out var bc))
                    foreach (var prop in bc.EnumerateObject())
                        config.BanCounts[prop.Name] = prop.Value.GetInt32();
            }
            catch { /* corrupt file — return empty config, don't crash */ }
        }

        if (string.IsNullOrEmpty(config.TeamName))
            config.TeamName = folderName;

        return config;
    }

    public void SaveTeam(TeamConfig team)
    {
        var dir  = Path.Combine(_config.ResourcesPath, "Teams", team.FolderName);
        var path = Path.Combine(dir, "BanData.json");

        // Ensure any gods in the current CharactersList are present — forward-compat
        // for newly added gods that weren't in the file when it was created.
        foreach (var god in LoadGodList())
            team.BanCounts.TryAdd(god, 0);

        var payload = new
        {
            bancounts  = team.BanCounts,
            totalgames = team.TotalGames,
            teamname   = team.TeamName
        };
        File.WriteAllText(path, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void SaveRoster(TeamConfig team)
    {
        var dir  = Path.Combine(_config.ResourcesPath, "Teams", team.FolderName);
        var path = Path.Combine(dir, "Roster.txt");
        File.WriteAllLines(path, team.Roster);
    }

    // ── Images ────────────────────────────────────────────────────────────

    public BitmapImage? GetPickImage(string godName)   => LoadImage(Path.Combine("CharacterImages", "Picks",   godName + ".png"));
    public BitmapImage? GetBanImage(string godName)    => LoadImage(Path.Combine("CharacterImages", "Bans",    godName + ".png"));
    public BitmapImage? GetTopBanImage(string godName) => LoadImage(Path.Combine("CharacterImages", "TopBans", godName + ".png"));

    private BitmapImage? LoadImage(string relativePath)
    {
        if (_imageCache.TryGetValue(relativePath, out var cached))
            return cached;

        var fullPath = Path.Combine(_config.ResourcesPath, relativePath);
        if (!File.Exists(fullPath)) return null;

        try
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource   = new Uri(fullPath, UriKind.Absolute);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();
            _imageCache[relativePath] = img;
            return img;
        }
        catch { return null; }
    }

    // ── Sounds ────────────────────────────────────────────────────────────

    public string? GetGodSoundPath(string godName)
    {
        var path = Path.Combine(_config.ResourcesPath, "Sounds", godName + ".mp3");
        return File.Exists(path) ? path : null;
    }

    public string? GetHoverSoundPath()
    {
        // Supports both .wav (original) and .mp3
        foreach (var name in new[] { "hover.wav", "hover.mp3" })
        {
            var path = Path.Combine(_config.ResourcesPath, "Sounds", name);
            if (File.Exists(path)) return path;
        }
        return null;
    }

    public string? GetLockInSoundPath()
    {
        foreach (var name in new[] { "lockin.wav", "lockin.mp3" })
        {
            var path = Path.Combine(_config.ResourcesPath, "Sounds", name);
            if (File.Exists(path)) return path;
        }
        return null;
    }

    // ── Layout ────────────────────────────────────────────────────────────

    public LayoutConfig LoadLayout()
    {
        var path = Path.Combine(_config.ResourcesPath, "Display", "layout.json");
        if (!File.Exists(path)) return new LayoutConfig();
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<LayoutConfig>(json, _jsonOpts) ?? new LayoutConfig();
        }
        catch { return new LayoutConfig(); }
    }

    public void SaveLayout(LayoutConfig layout)
    {
        var dir  = Path.Combine(_config.ResourcesPath, "Display");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "layout.json");
        File.WriteAllText(path, JsonSerializer.Serialize(layout, new JsonSerializerOptions { WriteIndented = true }));
    }
}
