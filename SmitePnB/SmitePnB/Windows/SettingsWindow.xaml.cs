using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using SmitePnB.Models;

namespace SmitePnB.Windows;

public partial class SettingsWindow : Window
{
    private static readonly string ConfigPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    private AppConfig    _config = new();
    private LayoutConfig _layout = new();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _config = App.Loader.Config;
        _layout = App.Loader.LoadLayout();

        TxtResourcesPath.Text = _config.ResourcesPath;
        CmbResolution.SelectedIndex = _config.ResolutionIndex;

        // Populate font list with installed system fonts + current value
        var fonts = System.Windows.Media.Fonts.SystemFontFamilies
                          .Select(f => f.Source)
                          .OrderBy(n => n)
                          .ToList();
        CmbFont.ItemsSource = fonts;
        CmbFont.Text = _config.FontFamily;

        TxtGodNameColor.Text  = _config.GodNameColor;
        TxtTeamNameColor.Text = _config.TeamNameColor;
        TxtScoreColor.Text    = _config.ScoreColor;

        TxtRoleLabels.Text = string.Join("\n", _layout.RoleLabels);

        UpdateSwatches();
    }

    // ── Browse ────────────────────────────────────────────────────────────

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description         = "Select the Resources folder",
            UseDescriptionForTitle = true,
            SelectedPath        = TxtResourcesPath.Text
        };
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            TxtResourcesPath.Text = dlg.SelectedPath;
    }

    // ── Font ──────────────────────────────────────────────────────────────

    private void CmbFont_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Live preview handled on Save
    }

    // ── Color swatches ────────────────────────────────────────────────────

    private void ColorInput_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        => UpdateSwatches();

    private void UpdateSwatches()
    {
        SwatchGodName.Fill  = BrushFromHex(TxtGodNameColor.Text);
        SwatchTeamName.Fill = BrushFromHex(TxtTeamNameColor.Text);
        SwatchScore.Fill    = BrushFromHex(TxtScoreColor.Text);
    }

    private static SolidColorBrush BrushFromHex(string hex)
    {
        try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)); }
        catch { return Brushes.Transparent; }
    }

    // ── Layout helpers ────────────────────────────────────────────────────

    private void BtnOpenLayout_Click(object sender, RoutedEventArgs e)
    {
        var path = Path.Combine(_config.ResourcesPath, "Display", "layout.json");
        if (!File.Exists(path))
        {
            App.Loader.SaveLayout(new LayoutConfig());
        }
        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
        catch { System.Windows.MessageBox.Show("Could not open file.", "Error"); }
    }

    private void BtnResetLayout_Click(object sender, RoutedEventArgs e)
    {
        if (System.Windows.MessageBox.Show(
            "Reset layout.json to default positions?", "Reset Layout",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            App.Loader.SaveLayout(new LayoutConfig());
        }
    }

    // ── Save / Cancel ─────────────────────────────────────────────────────

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        _config.ResourcesPath  = TxtResourcesPath.Text.Trim();
        _config.ResolutionIndex = CmbResolution.SelectedIndex;
        _config.FontFamily     = CmbFont.Text.Trim();
        _config.GodNameColor   = TxtGodNameColor.Text.Trim();
        _config.TeamNameColor  = TxtTeamNameColor.Text.Trim();
        _config.ScoreColor     = TxtScoreColor.Text.Trim();

        // Validate resources path before saving
        var problems = App.Loader.VerifyResources();
        if (problems.Count > 0)
        {
            TxtVerifyResult.Text       = "⚠ " + string.Join("\n⚠ ", problems);
            TxtVerifyResult.Visibility = Visibility.Visible;
            return;
        }
        TxtVerifyResult.Visibility = Visibility.Collapsed;

        // Role labels
        var roles = TxtRoleLabels.Text
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim())
                        .Where(l => l.Length > 0)
                        .ToArray();
        if (roles.Length == 5)
            _layout.RoleLabels = roles;

        App.Loader.SaveConfig(ConfigPath);
        App.Loader.SaveLayout(_layout);

        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
