using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SmitePnB.Models;

namespace SmitePnB.Windows;

/// <summary>
/// Green-screen in-game overlay window — captured by OBS via window_capture ("In-Game Overlay").
/// Shows current team art and locked picks over a #00FF00 chroma-key background.
/// Receives updates by calling Refresh() from MainWindow.
/// </summary>
public partial class InGameOverlayWindow : Window
{
    private LayoutConfig _layout = new();
    private AppConfig    _config = new();

    private readonly PickSlotControl[] _leftPicks  = new PickSlotControl[5];
    private readonly PickSlotControl[] _rightPicks = new PickSlotControl[5];

    public InGameOverlayWindow()
    {
        InitializeComponent();
        BuildSlots();
    }

    // ── Init ──────────────────────────────────────────────────────────────

    private void BuildSlots()
    {
        for (int i = 0; i < 5; i++)
        {
            _leftPicks[i]  = new PickSlotControl(); RootCanvas.Children.Add(_leftPicks[i]);
            _rightPicks[i] = new PickSlotControl(); RootCanvas.Children.Add(_rightPicks[i]);
        }
    }

    // ── Layout ────────────────────────────────────────────────────────────

    public void ApplyLayout(LayoutConfig layout, AppConfig config)
    {
        _layout = layout;
        _config = config;

        var (w, h) = AppConfig.GetResolutionSize(config.ResolutionIndex);
        Width  = w;
        Height = h;

        // Team art panel sizes and positions
        LeftArtImg.Width    = layout.OverlayPanelSize.Width;
        LeftArtImg.Height   = layout.OverlayPanelSize.Height;
        RightArtImg.Width   = layout.OverlayPanelSize.Width;
        RightArtImg.Height  = layout.OverlayPanelSize.Height;
        SetCanvasPos(LeftArtImg,  layout.OverlayLeftPanel);
        SetCanvasPos(RightArtImg, layout.OverlayRightPanel);

        // Team names and scores
        SetCanvasPos(LeftTeamName,  layout.OverlayLeftTeamName);
        SetCanvasPos(RightTeamName, layout.OverlayRightTeamName);
        SetCanvasPos(LeftScore,     layout.OverlayLeftScore);
        SetCanvasPos(RightScore,    layout.OverlayRightScore);

        // Pick slot sizes and positions
        var leftPickPoints  = new[] { layout.OverlayLeftPick1,  layout.OverlayLeftPick2,  layout.OverlayLeftPick3,  layout.OverlayLeftPick4,  layout.OverlayLeftPick5 };
        var rightPickPoints = new[] { layout.OverlayRightPick1, layout.OverlayRightPick2, layout.OverlayRightPick3, layout.OverlayRightPick4, layout.OverlayRightPick5 };

        for (int i = 0; i < 5; i++)
        {
            _leftPicks[i].SetSize(layout.OverlayPickSize.Width, layout.OverlayPickSize.Height);
            _rightPicks[i].SetSize(layout.OverlayPickSize.Width, layout.OverlayPickSize.Height);
            SetCanvasPos(_leftPicks[i],  leftPickPoints[i]);
            SetCanvasPos(_rightPicks[i], rightPickPoints[i]);
        }

        // Typography
        var font       = new FontFamily(config.FontFamily);
        var teamColor  = BrushFromHex(config.TeamNameColor);
        var scoreColor = BrushFromHex(config.ScoreColor);

        LeftTeamName.FontFamily  = font; LeftTeamName.Foreground  = teamColor;
        RightTeamName.FontFamily = font; RightTeamName.Foreground = teamColor;
        LeftScore.FontFamily     = font; LeftScore.Foreground     = scoreColor;
        RightScore.FontFamily    = font; RightScore.Foreground    = scoreColor;

        foreach (var s in _leftPicks.Concat(_rightPicks))
            s.SetFont(font, BrushFromHex(config.GodNameColor));
    }

    // ── Refresh ───────────────────────────────────────────────────────────

    /// <summary>
    /// Called from MainWindow after any state change. Updates team art, names,
    /// scores, and locked pick portraits.
    /// </summary>
    public void Refresh(DraftState state, TeamConfig? teamOne, TeamConfig? teamTwo, bool showGodNames)
    {
        LeftTeamName.Text  = state.TeamOneName;
        RightTeamName.Text = state.TeamTwoName;
        LeftScore.Text     = state.TeamOneScore.ToString();
        RightScore.Text    = state.TeamTwoScore.ToString();

        // Team art panels (Left.png / Right.png from each team folder)
        LeftArtImg.Source  = teamOne is not null ? App.Loader.GetTeamArtLeft(teamOne.FolderName)   : null;
        RightArtImg.Source = teamTwo is not null ? App.Loader.GetTeamArtRight(teamTwo.FolderName)  : null;

        for (int i = 0; i < 5; i++)
        {
            UpdatePickSlot(_leftPicks[i],  state.TeamOnePicks[i], showGodNames);
            UpdatePickSlot(_rightPicks[i], state.TeamTwoPicks[i], showGodNames);
        }
    }

    private void UpdatePickSlot(PickSlotControl ctrl, PickSlot slot, bool showName)
    {
        if (slot.IsLocked)
        {
            ctrl.SetImage(App.Loader.GetPickImage(slot.GodName));
            ctrl.SetName(showName ? slot.GodName : null);
        }
        else
        {
            ctrl.SetImage(null);
            ctrl.SetName(null);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void SetCanvasPos(UIElement el, LayoutPoint p)
    {
        Canvas.SetLeft(el, p.X);
        Canvas.SetTop(el,  p.Y);
    }

    private static SolidColorBrush BrushFromHex(string hex)
    {
        try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)); }
        catch { return Brushes.White; }
    }
}
