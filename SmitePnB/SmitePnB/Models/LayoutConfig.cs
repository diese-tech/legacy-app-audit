namespace SmitePnB.Models;

public class LayoutConfig
{
    public SlotSize PickSlot  { get; set; } = new(335, 110);
    public SlotSize BanSlot   { get; set; } = new(165, 67);
    public SlotSize TeamPanel { get; set; } = new(650, 180);
    public SlotSize ScoreBox  { get; set; } = new(100, 60);

    // Left side (Team One) ban positions
    public LayoutPoint LeftBan1  { get; set; } = new(0,    180);
    public LayoutPoint LeftBan2  { get; set; } = new(0,    290);
    public LayoutPoint LeftBan3  { get; set; } = new(0,    400);
    public LayoutPoint LeftBan4  { get; set; } = new(0,    510);
    public LayoutPoint LeftBan5  { get; set; } = new(0,    620);

    // Right side (Team Two) ban positions
    public LayoutPoint RightBan1 { get; set; } = new(1265, 180);
    public LayoutPoint RightBan2 { get; set; } = new(1265, 290);
    public LayoutPoint RightBan3 { get; set; } = new(1265, 400);
    public LayoutPoint RightBan4 { get; set; } = new(1265, 510);
    public LayoutPoint RightBan5 { get; set; } = new(1265, 620);

    // Left side pick positions
    public LayoutPoint LeftPick1  { get; set; } = new(67,   735);
    public LayoutPoint LeftPick2  { get; set; } = new(134,  735);
    public LayoutPoint LeftPick3  { get; set; } = new(201,  735);
    public LayoutPoint LeftPick4  { get; set; } = new(268,  735);
    public LayoutPoint LeftPick5  { get; set; } = new(335,  735);

    // Right side pick positions
    public LayoutPoint RightPick1 { get; set; } = new(1533, 735);
    public LayoutPoint RightPick2 { get; set; } = new(1466, 735);
    public LayoutPoint RightPick3 { get; set; } = new(1399, 735);
    public LayoutPoint RightPick4 { get; set; } = new(1332, 735);
    public LayoutPoint RightPick5 { get; set; } = new(1265, 735);

    // Team background panels
    public LayoutPoint LeftTeamPanel  { get; set; } = new(0,    180);
    public LayoutPoint RightTeamPanel { get; set; } = new(1265, 180);

    // Score boxes
    public LayoutPoint LeftScore  { get; set; } = new(460,  0);
    public LayoutPoint RightScore { get; set; } = new(950,  0);

    // Team name labels
    public LayoutPoint LeftTeamName  { get; set; } = new(335, 700);
    public LayoutPoint RightTeamName { get; set; } = new(965, 700);

    // Top bans panels
    public LayoutPoint LeftTopBans  { get; set; } = new(0,   180);
    public LayoutPoint RightTopBans { get; set; } = new(950, 180);

    // Player name label positions (relative to their pick slot)
    public LayoutPoint PlayerNameOffset { get; set; } = new(0, 80);

    /// <summary>Role labels shown under player names. Editable so leagues can use custom role names.</summary>
    public string[] RoleLabels { get; set; } = ["SOLO", "JUNGLE", "MID", "SUPPORT", "CARRY"];
}

public record SlotSize(double Width, double Height);
public record LayoutPoint(double X, double Y);
