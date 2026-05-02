using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SmitePnB.Models;

namespace SmitePnB.Windows;

/// <summary>
/// The window OBS captures via window_capture.
/// Receives updates by calling Refresh() from MainWindow — no polling, no timers.
/// All layout positions come from LayoutConfig so the operator can reposition
/// elements without touching code.
/// </summary>
public partial class DisplayWindow : Window
{
    private LayoutConfig _layout = new();
    private AppConfig    _config = new();

    // Dynamically created slot controls indexed [0..4]
    private readonly BanSlotControl[]  _leftBans   = new BanSlotControl[5];
    private readonly BanSlotControl[]  _rightBans  = new BanSlotControl[5];
    private readonly PickSlotControl[] _leftPicks  = new PickSlotControl[5];
    private readonly PickSlotControl[] _rightPicks = new PickSlotControl[5];

    public DisplayWindow()
    {
        InitializeComponent();
        BuildSlots();
    }

    // ── Init ──────────────────────────────────────────────────────────────

    private void BuildSlots()
    {
        for (int i = 0; i < 5; i++)
        {
            _leftBans[i]   = new BanSlotControl();  RootCanvas.Children.Add(_leftBans[i]);
            _rightBans[i]  = new BanSlotControl();  RootCanvas.Children.Add(_rightBans[i]);
            _leftPicks[i]  = new PickSlotControl(); RootCanvas.Children.Add(_leftPicks[i]);
            _rightPicks[i] = new PickSlotControl(); RootCanvas.Children.Add(_rightPicks[i]);
        }
        RootCanvas.Children.Add(LeftTopBansPanel);
        RootCanvas.Children.Add(RightTopBansPanel);
    }

    // ── Layout ────────────────────────────────────────────────────────────

    public void ApplyLayout(LayoutConfig layout, AppConfig config)
    {
        _layout = layout;
        _config = config;

        var (w, h) = AppConfig.GetResolutionSize(config.ResolutionIndex);
        Width  = w;
        Height = h;

        // Apply slot sizes
        foreach (var s in _leftBans.Concat(_rightBans))
            s.SetSize(layout.BanSlot.Width, layout.BanSlot.Height);
        foreach (var s in _leftPicks.Concat(_rightPicks))
            s.SetSize(layout.PickSlot.Width, layout.PickSlot.Height);

        // Position ban slots
        var leftBanPoints  = new[] { layout.LeftBan1,  layout.LeftBan2,  layout.LeftBan3,  layout.LeftBan4,  layout.LeftBan5 };
        var rightBanPoints = new[] { layout.RightBan1, layout.RightBan2, layout.RightBan3, layout.RightBan4, layout.RightBan5 };
        for (int i = 0; i < 5; i++)
        {
            SetCanvasPos(_leftBans[i],  leftBanPoints[i]);
            SetCanvasPos(_rightBans[i], rightBanPoints[i]);
        }

        // Position pick slots
        var leftPickPoints  = new[] { layout.LeftPick1,  layout.LeftPick2,  layout.LeftPick3,  layout.LeftPick4,  layout.LeftPick5 };
        var rightPickPoints = new[] { layout.RightPick1, layout.RightPick2, layout.RightPick3, layout.RightPick4, layout.RightPick5 };
        for (int i = 0; i < 5; i++)
        {
            SetCanvasPos(_leftPicks[i],  leftPickPoints[i]);
            SetCanvasPos(_rightPicks[i], rightPickPoints[i]);
        }

        // Team panels, names, scores, top bans
        SetCanvasPos(LeftTeamName,     layout.LeftTeamName);
        SetCanvasPos(RightTeamName,    layout.RightTeamName);
        SetCanvasPos(LeftScore,        layout.LeftScore);
        SetCanvasPos(RightScore,       layout.RightScore);
        SetCanvasPos(LeftTopBansPanel, layout.LeftTopBans);
        SetCanvasPos(RightTopBansPanel,layout.RightTopBans);

        // Font and colors from AppConfig
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
    /// Called from MainWindow after any state change. Repaints the entire
    /// display from the current DraftState. Cheap enough to call on every
    /// operator click.
    /// </summary>
    public void Refresh(DraftState state, TeamConfig? teamOne, TeamConfig? teamTwo, bool showGodNames)
    {
        LeftTeamName.Text  = state.TeamOneName;
        RightTeamName.Text = state.TeamTwoName;
        LeftScore.Text     = state.TeamOneScore.ToString();
        RightScore.Text    = state.TeamTwoScore.ToString();

        for (int i = 0; i < 5; i++)
        {
            UpdateBanSlot(_leftBans[i],   state.TeamOneBans[i]);
            UpdateBanSlot(_rightBans[i],  state.TeamTwoBans[i]);
            UpdatePickSlot(_leftPicks[i],  state.TeamOnePicks[i], showGodNames);
            UpdatePickSlot(_rightPicks[i], state.TeamTwoPicks[i], showGodNames);
        }

        RefreshTopBans(teamOne, teamTwo);
    }

    private void UpdateBanSlot(BanSlotControl ctrl, BanSlot slot)
    {
        if (slot.IsLocked)
        {
            ctrl.SetImage(App.Resources.GetBanImage(slot.GodName));
            ctrl.SetState(BanSlotState.Locked);
        }
        else if (slot.IsHovered)
        {
            ctrl.SetImage(App.Resources.GetBanImage(slot.GodName));
            ctrl.SetState(BanSlotState.Hovered);
        }
        else
        {
            ctrl.SetImage(null);
            ctrl.SetState(BanSlotState.Empty);
        }
    }

    private void UpdatePickSlot(PickSlotControl ctrl, PickSlot slot, bool showName)
    {
        if (slot.IsLocked)
        {
            ctrl.SetImage(App.Resources.GetPickImage(slot.GodName));
            ctrl.SetName(showName ? slot.GodName : null);
        }
        else
        {
            ctrl.SetImage(null);
            ctrl.SetName(null);
        }
    }

    private void RefreshTopBans(TeamConfig? teamOne, TeamConfig? teamTwo)
    {
        PopulateTopBans(LeftTopBansPanel,  teamOne);
        PopulateTopBans(RightTopBansPanel, teamTwo);
    }

    private void PopulateTopBans(StackPanel panel, TeamConfig? team)
    {
        panel.Children.Clear();
        if (team is null) { panel.Visibility = Visibility.Collapsed; return; }
        panel.Visibility = Visibility.Visible;
        foreach (var (godName, _) in team.GetTopBans(6))
        {
            var img = App.Resources.GetTopBanImage(godName);
            if (img is null) continue;
            panel.Children.Add(new Image
            {
                Source = img,
                Width  = _layout.BanSlot.Width,
                Height = _layout.BanSlot.Height,
                Stretch = Stretch.Uniform
            });
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void SetCanvasPos(UIElement el, Models.Point p)
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

// ── Inline slot controls ──────────────────────────────────────────────────

public enum BanSlotState { Empty, Hovered, Locked }

/// <summary>A single ban image slot on the display. Border color indicates hover vs locked state.</summary>
public class BanSlotControl : Grid
{
    private readonly Image  _image  = new() { Stretch = Stretch.Uniform };
    private readonly Border _border = new() { BorderThickness = new Thickness(2) };

    public BanSlotControl()
    {
        _border.Child = _image;
        Children.Add(_border);
        SetState(BanSlotState.Empty);
    }

    public void SetSize(double w, double h) { Width = w; Height = h; }

    public void SetImage(BitmapImage? img)
    {
        _image.Source = img;
        Visibility = img is null ? Visibility.Hidden : Visibility.Visible;
    }

    public void SetState(BanSlotState state)
    {
        _border.BorderBrush = state switch
        {
            BanSlotState.Hovered => Brushes.Gold,
            BanSlotState.Locked  => Brushes.Crimson,
            _                    => Brushes.Transparent,
        };
        if (state == BanSlotState.Empty) Visibility = Visibility.Hidden;
    }
}

/// <summary>A single pick image slot with an optional god name overlay.</summary>
public class PickSlotControl : Grid
{
    private readonly Image     _image   = new() { Stretch = Stretch.UniformToFill };
    private readonly TextBlock _nameTag = new()
    {
        VerticalAlignment   = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Center,
        TextAlignment       = TextAlignment.Center,
        FontSize            = 14,
        Foreground          = Brushes.White,
        Visibility          = Visibility.Collapsed
    };

    public PickSlotControl()
    {
        Children.Add(_image);
        Children.Add(_nameTag);
        Visibility = Visibility.Hidden;
    }

    public void SetSize(double w, double h) { Width = w; Height = h; }

    public void SetImage(BitmapImage? img)
    {
        _image.Source = img;
        Visibility = img is null ? Visibility.Hidden : Visibility.Visible;
    }

    public void SetName(string? name)
    {
        if (string.IsNullOrEmpty(name)) { _nameTag.Visibility = Visibility.Collapsed; return; }
        _nameTag.Text       = name;
        _nameTag.Visibility = Visibility.Visible;
    }

    public void SetFont(FontFamily font, Brush color)
    {
        _nameTag.FontFamily = font;
        _nameTag.Foreground = color;
    }
}
