namespace SmitePnB.Models;

/// <summary>
/// Persisted application configuration. Stored as config.json next to the exe.
/// Replaces the old Data.txt key/value flat file.
/// </summary>
public class AppConfig
{
    /// <summary>Absolute path to the Resources folder (gods, teams, sounds, images).</summary>
    public string ResourcesPath { get; set; } = string.Empty;

    /// <summary>
    /// 0 = 1920x1080 (default)
    /// 1 = 2560x1440
    /// Determines the default display window size.
    /// </summary>
    public int ResolutionIndex { get; set; } = 0;

    /// <summary>Show god names as text overlaid on pick slot images.</summary>
    public bool ShowGodNames { get; set; } = true;

    /// <summary>Display window font family for all text elements.</summary>
    public string FontFamily { get; set; } = "Impact";

    /// <summary>Hex color for god name text on picks, e.g. "#FFFFFF".</summary>
    public string GodNameColor { get; set; } = "#FFFFFF";

    /// <summary>Hex color for team name text.</summary>
    public string TeamNameColor { get; set; } = "#FFFFFF";

    /// <summary>Hex color for score text.</summary>
    public string ScoreColor { get; set; } = "#FFFFFF";

    public static (int Width, int Height) GetResolutionSize(int index) => index switch
    {
        1 => (1920, 1080),
        2 => (2560, 1440),
        _ => (1600, 900),   // default
    };
}
