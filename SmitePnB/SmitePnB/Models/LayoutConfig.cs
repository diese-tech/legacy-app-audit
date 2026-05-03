namespace SmitePnB.Models;

public class LayoutConfig
{
    // Pick images: wide landscape cards stacked vertically on each side panel
    public SlotSize PickSlot  { get; set; } = new(310, 120);
    // Ban images: smaller cards in a horizontal row at the bottom
    public SlotSize BanSlot   { get; set; } = new(90, 70);
    public SlotSize TeamPanel { get; set; } = new(310, 900);
    public SlotSize ScoreBox  { get; set; } = new(100, 60);

    // Left side (Team One) pick positions — stacked vertically in left panel
    public LayoutPoint LeftPick1  { get; set; } = new(0,    50);
    public LayoutPoint LeftPick2  { get; set; } = new(0,   180);
    public LayoutPoint LeftPick3  { get; set; } = new(0,   310);
    public LayoutPoint LeftPick4  { get; set; } = new(0,   440);
    public LayoutPoint LeftPick5  { get; set; } = new(0,   570);

    // Right side (Team Two) pick positions — stacked vertically in right panel
    public LayoutPoint RightPick1 { get; set; } = new(1290, 50);
    public LayoutPoint RightPick2 { get; set; } = new(1290, 180);
    public LayoutPoint RightPick3 { get; set; } = new(1290, 310);
    public LayoutPoint RightPick4 { get; set; } = new(1290, 440);
    public LayoutPoint RightPick5 { get; set; } = new(1290, 570);

    // Left side (Team One) ban positions — horizontal row at bottom
    public LayoutPoint LeftBan1  { get; set; } = new(20,  800);
    public LayoutPoint LeftBan2  { get; set; } = new(115, 800);
    public LayoutPoint LeftBan3  { get; set; } = new(210, 800);
    public LayoutPoint LeftBan4  { get; set; } = new(305, 800);
    public LayoutPoint LeftBan5  { get; set; } = new(400, 800);

    // Right side (Team Two) ban positions — horizontal row at bottom (mirrored)
    public LayoutPoint RightBan1 { get; set; } = new(1110, 800);
    public LayoutPoint RightBan2 { get; set; } = new(1205, 800);
    public LayoutPoint RightBan3 { get; set; } = new(1300, 800);
    public LayoutPoint RightBan4 { get; set; } = new(1395, 800);
    public LayoutPoint RightBan5 { get; set; } = new(1490, 800);

    // Team background panels
    public LayoutPoint LeftTeamPanel  { get; set; } = new(0,    0);
    public LayoutPoint RightTeamPanel { get; set; } = new(1290, 0);

    // Score boxes (center)
    public LayoutPoint LeftScore  { get; set; } = new(670, 10);
    public LayoutPoint RightScore { get; set; } = new(860, 10);

    // Team name labels
    public LayoutPoint LeftTeamName  { get; set; } = new(20,  15);
    public LayoutPoint RightTeamName { get; set; } = new(1310, 15);

    // Top bans panels
    public LayoutPoint LeftTopBans  { get; set; } = new(315, 10);
    public LayoutPoint RightTopBans { get; set; } = new(950, 10);

    // Player name label positions (relative to their pick slot)
    public LayoutPoint PlayerNameOffset { get; set; } = new(0, 85);

    // P/B Display — team-specific side panel images (PnBLeft.png / PnBRight.png)
    public LayoutPoint PnBLeftPanel  { get; set; } = new(0,    0);
    public LayoutPoint PnBRightPanel { get; set; } = new(1290, 0);
    public SlotSize    PnBPanelSize  { get; set; } = new(310, 900);

    // In-Game Overlay — team art panel positions (Left.png / Right.png)
    public LayoutPoint OverlayLeftPanel  { get; set; } = new(0,    0);
    public LayoutPoint OverlayRightPanel { get; set; } = new(1290, 0);
    public SlotSize    OverlayPanelSize  { get; set; } = new(310, 900);

    // In-Game Overlay — pick portrait positions (5 per side, stacked vertically inside panels)
    public LayoutPoint OverlayLeftPick1  { get; set; } = new(5,    50);
    public LayoutPoint OverlayLeftPick2  { get; set; } = new(5,   180);
    public LayoutPoint OverlayLeftPick3  { get; set; } = new(5,   310);
    public LayoutPoint OverlayLeftPick4  { get; set; } = new(5,   440);
    public LayoutPoint OverlayLeftPick5  { get; set; } = new(5,   570);
    public LayoutPoint OverlayRightPick1 { get; set; } = new(1295, 50);
    public LayoutPoint OverlayRightPick2 { get; set; } = new(1295, 180);
    public LayoutPoint OverlayRightPick3 { get; set; } = new(1295, 310);
    public LayoutPoint OverlayRightPick4 { get; set; } = new(1295, 440);
    public LayoutPoint OverlayRightPick5 { get; set; } = new(1295, 570);
    public SlotSize    OverlayPickSize   { get; set; } = new(300, 120);

    // In-Game Overlay — team name and score positions
    public LayoutPoint OverlayLeftTeamName  { get; set; } = new(20,   15);
    public LayoutPoint OverlayRightTeamName { get; set; } = new(1310, 15);
    public LayoutPoint OverlayLeftScore     { get; set; } = new(670,  10);
    public LayoutPoint OverlayRightScore    { get; set; } = new(860,  10);

    /// <summary>Role labels shown under player names. Editable so leagues can use custom role names.</summary>
    public string[] RoleLabels { get; set; } = ["SOLO", "JUNGLE", "MID", "SUPPORT", "CARRY"];
}

public record SlotSize(double Width, double Height);
public record LayoutPoint(double X, double Y);
