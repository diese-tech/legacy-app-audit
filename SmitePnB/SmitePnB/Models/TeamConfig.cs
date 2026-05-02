namespace SmitePnB.Models;

/// <summary>
/// Everything loaded from a team's folder under Resources/Teams/.
/// Combines Roster.txt (player names) and BanData.json (historical ban stats).
/// </summary>
public class TeamConfig
{
    /// <summary>Display name — from BanData.json "teamname" field.</summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>Folder name on disk — used to reload and save BanData.json.</summary>
    public string FolderName { get; set; } = string.Empty;

    /// <summary>Five player names in role order: Solo, Jungle, Mid, Support, Carry.</summary>
    public string[] Roster { get; set; } = ["Solo", "Jungle", "Mid", "Support", "Carry"];

    /// <summary>Per-god ban counts across all tracked games.</summary>
    public Dictionary<string, int> BanCounts { get; set; } = [];

    public int TotalGames { get; set; }

    /// <summary>Returns the N most-banned gods descending by count. Used for the top-bans display.</summary>
    public List<(string GodName, int Count)> GetTopBans(int n = 6)
    {
        return BanCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(n)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }

    /// <summary>
    /// Increments ban counts for every locked ban in the provided slots and
    /// increments TotalGames by 1. Called on "Submit Ban Data".
    /// </summary>
    public void RecordGame(IEnumerable<BanSlot> bans)
    {
        TotalGames++;
        foreach (var ban in bans.Where(b => b.IsLocked && !string.IsNullOrEmpty(b.GodName)))
        {
            if (BanCounts.ContainsKey(ban.GodName))
                BanCounts[ban.GodName]++;
            // Silently skip gods not in the tracked list — they may be newly added
            // gods that haven't been added to this team's BanData.json yet.
            // The ResourceLoader will add missing gods on next load.
        }
    }
}
