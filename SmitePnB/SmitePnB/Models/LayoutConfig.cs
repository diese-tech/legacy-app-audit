namespace SmitePnB.Models;

/// <summary>
/// Pixel positions and sizes for every element on the display window.
/// Stored as layout.json in Resources/Display/ — human-readable and directly editable.
///
/// All coordinates are relative to the top-left of the display window.
/// X = distance from left edge, Y = distance from top edge.
/// </summary>
public class LayoutConfig
{
    public SlotSize PickSlot  { get; set; } = new(335, 110);
    public SlotSize BanSlot   { get; set; } = new(165, 67);
    public SlotSize TeamPanel { get; set; } = new(650, 180);
    public SlotSize ScoreBox  { get; set; } = new(100, 60);

    // Left side (Team One) ban positions
    public Point LeftBan1  { get; set; } = new(0,    180);
    public Point LeftBan2  { get; set; } = new(0,    290);
    public Point LeftBan3  { get; set; } = new(0,    400);
    public Point LeftBan4  { get; set; } = new(0,    510);
    public Point LeftBan5  { get; set; } = new(0,    620);

    // Right side (Team Two) ban positions
    public Point RightBan1 { get; set; } = new(1265, 180);
    public Point RightBan2 { get; set; } = new(1265, 290);
    public Point RightBan3 { get; set; } = new(1265, 400);
    public Point RightBan4 { get; set; } = new(1265, 510);
    public Point RightBan5 { get; set; } = new(1265, 620);

    // Left side pick positions
    public Point LeftPick1  { get; set; } = new(67,   735);
    public Point LeftPick2  { get; set; } = new(134,  735);
    public Point LeftPick3  { get; set; } = new(201,  735);
    public Point LeftPick4  { get; set; } = new(268,  735);
    public Point LeftPick5  { get; set; } = new(335,  735);

    // Right side pick positions
    public Point RightPick1 { get; set; } = new(1533, 735);
    public Point RightPick2 { get; set; } = new(1466, 735);
    public Point RightPick3 { get; set; } = new(1399, 735);
    public Point RightPick4 { get; set; } = new(1332, 735);
    public Point RightPick5 { get; set; } = new(1265, 735);

    // Team background panels
    public Point LeftTeamPanel  { get; set; } = new(0,    180);
    public Point RightTeamPanel { get; set; } = new(1265, 180);

    // Score boxes
    public Point LeftScore  { get; set; } = new(460,  0);
    public Point RightScore { get; set; } = new(950,  0);

    // Team name labels
    public Point LeftTeamName  { get; set; } = new(335, 700);
    public Point RightTeamName { get; set; } = new(965, 700);

    // Top bans panels
    public Point LeftTopBans  { get; set; } = new(0,   180);
    public Point RightTopBans { get; set; } = new(950, 180);

    // Player name label positions (relative to their pick slot)
    public Point PlayerNameOffset { get; set; } = new(0, 80);

    /// <summary>Role labels shown under player names. Editable so leagues can use custom role names.</summary>
    public string[] RoleLabels { get; set; } = ["SOLO", "JUNGLE", "MID", "SUPPORT", "CARRY"];
}

public record SlotSize(double Width, double Height);
public record Point(double X, double Y);
