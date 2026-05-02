namespace SmitePnB.Models;

/// <summary>
/// Complete state of an in-progress draft. Serialized to disk on every lock-in
/// so a crash mid-draft can be recovered without re-entering from memory.
/// </summary>
public class DraftState
{
    public string TeamOneName       { get; set; } = string.Empty;
    public string TeamTwoName       { get; set; } = string.Empty;
    public string TeamOneFolderName { get; set; } = string.Empty;
    public string TeamTwoFolderName { get; set; } = string.Empty;
    public int TeamOneScore  { get; set; }
    public int TeamTwoScore  { get; set; }

    public PickSlot[] TeamOnePicks { get; set; } = Enumerable.Range(0, 5).Select(_ => new PickSlot()).ToArray();
    public PickSlot[] TeamTwoPicks { get; set; } = Enumerable.Range(0, 5).Select(_ => new PickSlot()).ToArray();
    public BanSlot[]  TeamOneBans  { get; set; } = Enumerable.Range(0, 5).Select(_ => new BanSlot()).ToArray();
    public BanSlot[]  TeamTwoBans  { get; set; } = Enumerable.Range(0, 5).Select(_ => new BanSlot()).ToArray();

    /// <summary>Timestamp of last autosave — shown in the restore prompt so the operator knows how stale it is.</summary>
    public DateTime SavedAt { get; set; }

    public void Clear()
    {
        TeamOneName       = string.Empty;
        TeamTwoName       = string.Empty;
        TeamOneFolderName = string.Empty;
        TeamTwoFolderName = string.Empty;
        TeamOneScore = 0;
        TeamTwoScore = 0;
        foreach (var s in TeamOnePicks) s.Clear();
        foreach (var s in TeamTwoPicks) s.Clear();
        foreach (var s in TeamOneBans)  s.Clear();
        foreach (var s in TeamTwoBans)  s.Clear();
    }
}

public class PickSlot
{
    public string GodName { get; set; } = string.Empty;
    public bool   IsLocked { get; set; }

    public void Clear() { GodName = string.Empty; IsLocked = false; }
}

public class BanSlot
{
    public string GodName   { get; set; } = string.Empty;
    /// <summary>Hover state — operator can show/hide before committing the ban to air.</summary>
    public bool   IsHovered { get; set; }
    public bool   IsLocked  { get; set; }

    public void Clear() { GodName = string.Empty; IsHovered = false; IsLocked = false; }
}
