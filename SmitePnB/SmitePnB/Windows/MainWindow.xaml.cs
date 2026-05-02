using System.Windows;
using System.Windows.Controls;
using SmitePnB.Models;
using SmitePnB.Services;

namespace SmitePnB.Windows;

public partial class MainWindow : Window
{
    private static readonly string ConfigPath =
        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    private DraftState   _state   = new();
    private TeamConfig?  _teamOne;
    private TeamConfig?  _teamTwo;
    private LayoutConfig _layout  = new();
    private List<string> _gods    = [];
    private DisplayWindow?        _display;
    private InGameOverlayWindow?  _overlay;

    // View models bound to the ItemsControls
    private readonly List<BanRowVm>  _t1Bans  = [];
    private readonly List<BanRowVm>  _t2Bans  = [];
    private readonly List<PickRowVm> _t1Picks = [];
    private readonly List<PickRowVm> _t2Picks = [];

    private bool _suppressRefresh;

    // ── Startup ───────────────────────────────────────────────────────────

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        App.Loader.Load(ConfigPath);

        // Prompt for resources path if not yet configured
        if (string.IsNullOrEmpty(App.Loader.Config.ResourcesPath) ||
            App.Loader.VerifyResources().Count > 0)
        {
            var settings = new SettingsWindow();
            settings.ShowDialog();
            App.Loader.Load(ConfigPath);
        }

        _gods   = App.Loader.LoadGodList();
        _layout = App.Loader.LoadLayout();

        PopulateTeamCombos();
        BuildRowViewModels();

        // Open the two stream-facing output windows
        _display = new DisplayWindow();
        _display.ApplyLayout(_layout, App.Loader.Config);
        _display.Show();

        _overlay = new InGameOverlayWindow();
        _overlay.ApplyLayout(_layout, App.Loader.Config);
        _overlay.Show();

        // Offer to restore an autosaved draft
        var saved = App.State.TryLoadAutosave();
        if (saved is not null)
        {
            var msg = $"A draft was autosaved at {saved.SavedAt:HH:mm:ss}.\n\n" +
                      $"{saved.TeamOneName} vs {saved.TeamTwoName}\n\nRestore it?";
            if (MessageBox.Show(msg, "Restore Draft", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                RestoreState(saved);
        }

        RefreshDisplay();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _display?.Close();
        _overlay?.Close();
    }

    // ── Team population ───────────────────────────────────────────────────

    private void PopulateTeamCombos()
    {
        var folders = App.Loader.GetTeamFolders();
        CmbTeamOne.ItemsSource = folders;
        CmbTeamTwo.ItemsSource = folders;
        if (folders.Count > 0) CmbTeamOne.SelectedIndex = 0;
        if (folders.Count > 1) CmbTeamTwo.SelectedIndex = 1;
    }

    private void CmbTeamOne_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (CmbTeamOne.SelectedItem is string folder)
        {
            _teamOne = App.Loader.LoadTeam(folder);
            _state.TeamOneName       = _teamOne.TeamName;
            _state.TeamOneFolderName = folder;
            RefreshDisplay();
        }
    }

    private void CmbTeamTwo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (CmbTeamTwo.SelectedItem is string folder)
        {
            _teamTwo = App.Loader.LoadTeam(folder);
            _state.TeamTwoName       = _teamTwo.TeamName;
            _state.TeamTwoFolderName = folder;
            RefreshDisplay();
        }
    }

    // ── Row view models ───────────────────────────────────────────────────

    private void BuildRowViewModels()
    {
        _t1Bans.Clear(); _t2Bans.Clear();
        _t1Picks.Clear(); _t2Picks.Clear();

        var roles = _layout.RoleLabels;
        for (int i = 0; i < 5; i++)
        {
            _t1Bans.Add(new BanRowVm  { Label = $"B{i+1}", Gods = _gods, Slot = _state.TeamOneBans[i]  });
            _t2Bans.Add(new BanRowVm  { Label = $"B{i+1}", Gods = _gods, Slot = _state.TeamTwoBans[i]  });
            _t1Picks.Add(new PickRowVm { Role = roles[i],  Gods = _gods, Slot = _state.TeamOnePicks[i] });
            _t2Picks.Add(new PickRowVm { Role = roles[i],  Gods = _gods, Slot = _state.TeamTwoPicks[i] });
        }

        TeamOneBanList.ItemsSource  = _t1Bans;
        TeamTwoBanList.ItemsSource  = _t2Bans;
        TeamOnePickList.ItemsSource = _t1Picks;
        TeamTwoPickList.ItemsSource = _t2Picks;
    }

    // ── Event handlers — Bans ─────────────────────────────────────────────

    private void BanGod_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressRefresh) return;
        if (sender is ComboBox cmb && cmb.Tag is BanRowVm vm)
        {
            vm.Slot.GodName = vm.GodName ?? string.Empty;
            App.Audio.PlayHover(App.Loader.GetHoverSoundPath());
        }
        RefreshDisplay();
    }

    private void BanHover_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox chk && chk.Tag is BanRowVm vm)
            vm.Slot.IsHovered = chk.IsChecked == true;
        RefreshDisplay();
    }

    private void BanLock_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox chk && chk.Tag is BanRowVm vm)
        {
            vm.Slot.IsLocked = chk.IsChecked == true;
            if (vm.Slot.IsLocked)
            {
                App.Audio.PlayLockIn(
                    App.Loader.GetLockInSoundPath(),
                    App.Loader.GetGodSoundPath(vm.Slot.GodName));
            }
        }
        Autosave();
        RefreshDisplay();
    }

    // ── Event handlers — Picks ────────────────────────────────────────────

    private void PickGod_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressRefresh) return;
        if (sender is ComboBox cmb && cmb.Tag is PickRowVm vm)
        {
            vm.Slot.GodName = vm.GodName ?? string.Empty;
            App.Audio.PlayHover(App.Loader.GetHoverSoundPath());
        }
        RefreshDisplay();
    }

    private void PickLock_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox chk && chk.Tag is PickRowVm vm)
        {
            vm.Slot.IsLocked = chk.IsChecked == true;
            if (vm.Slot.IsLocked)
            {
                App.Audio.PlayLockIn(
                    App.Loader.GetLockInSoundPath(),
                    App.Loader.GetGodSoundPath(vm.Slot.GodName));
            }
        }
        Autosave();
        RefreshDisplay();
    }

    // ── Toolbar actions ───────────────────────────────────────────────────

    private void BtnNewDraft_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Clear all picks and bans?", "New Draft",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        _state.Clear();
        // Re-apply team names and folder names since Clear() wipes them
        if (_teamOne is not null)
        {
            _state.TeamOneName       = _teamOne.TeamName;
            _state.TeamOneFolderName = CmbTeamOne.SelectedItem as string ?? string.Empty;
        }
        if (_teamTwo is not null)
        {
            _state.TeamTwoName       = _teamTwo.TeamName;
            _state.TeamTwoFolderName = CmbTeamTwo.SelectedItem as string ?? string.Empty;
        }

        App.State.DeleteAutosave();
        _suppressRefresh = true;
        BuildRowViewModels();
        _suppressRefresh = false;
        RefreshDisplay();
    }

    private void BtnSubmitBans_Click(object sender, RoutedEventArgs e)
    {
        var lockedOneBans = _state.TeamOneBans.Count(b => b.IsLocked);
        var lockedTwoBans = _state.TeamTwoBans.Count(b => b.IsLocked);

        if (lockedOneBans == 0 && lockedTwoBans == 0)
        {
            MessageBox.Show("No locked bans to submit.", "Submit Bans", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var msg = $"Submit ban data?\n\n" +
                  $"{_state.TeamOneName}: {lockedOneBans} ban(s)\n" +
                  $"{_state.TeamTwoName}: {lockedTwoBans} ban(s)";

        if (MessageBox.Show(msg, "Submit Bans", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        if (_teamOne is not null)
        {
            _teamOne.RecordGame(_state.TeamOneBans);
            App.Loader.SaveTeam(_teamOne);
        }
        if (_teamTwo is not null)
        {
            _teamTwo.RecordGame(_state.TeamTwoBans);
            App.Loader.SaveTeam(_teamTwo);
        }

        App.State.DeleteAutosave();
        MessageBox.Show("Ban data saved.", "Submit Bans", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnSwapTeams_Click(object sender, RoutedEventArgs e)
    {
        var idxOne = CmbTeamOne.SelectedIndex;
        var idxTwo = CmbTeamTwo.SelectedIndex;
        CmbTeamOne.SelectedIndex = idxTwo;
        CmbTeamTwo.SelectedIndex = idxOne;
    }

    private void BtnResetTeamOneBans_Click(object sender, RoutedEventArgs e)
    {
        foreach (var b in _state.TeamOneBans) b.Clear();
        _suppressRefresh = true;
        foreach (var vm in _t1Bans) { vm.GodName = null; vm.IsHovered = false; vm.IsLocked = false; }
        _suppressRefresh = false;
        RefreshDisplay();
    }

    private void BtnResetTeamTwoBans_Click(object sender, RoutedEventArgs e)
    {
        foreach (var b in _state.TeamTwoBans) b.Clear();
        _suppressRefresh = true;
        foreach (var vm in _t2Bans) { vm.GodName = null; vm.IsHovered = false; vm.IsLocked = false; }
        _suppressRefresh = false;
        RefreshDisplay();
    }

    private void BtnResetTeamOnePicks_Click(object sender, RoutedEventArgs e)
    {
        foreach (var p in _state.TeamOnePicks) p.Clear();
        _suppressRefresh = true;
        foreach (var vm in _t1Picks) { vm.GodName = null; vm.IsLocked = false; }
        _suppressRefresh = false;
        RefreshDisplay();
    }

    private void BtnResetTeamTwoPicks_Click(object sender, RoutedEventArgs e)
    {
        foreach (var p in _state.TeamTwoPicks) p.Clear();
        _suppressRefresh = true;
        foreach (var vm in _t2Picks) { vm.GodName = null; vm.IsLocked = false; }
        _suppressRefresh = false;
        RefreshDisplay();
    }

    private void Score_Changed(object sender, TextChangedEventArgs e)
    {
        if (TxtLeftScore is null || TxtRightScore is null)
            return;

        if (int.TryParse(TxtLeftScore.Text, out var l)) _state.TeamOneScore = l;
        if (int.TryParse(TxtRightScore.Text, out var r)) _state.TeamTwoScore = r;
        RefreshDisplay();
    }

    private void ChkShowGodNames_Changed(object sender, RoutedEventArgs e)
        => RefreshDisplay();

    private void BtnGodManager_Click(object sender, RoutedEventArgs e)
    {
        var win = new GodManagerWindow { Owner = this };
        win.ShowDialog();
        if (win.GodListChanged)
        {
            _gods = App.Loader.LoadGodList();
            BuildRowViewModels();
        }
    }

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        var win = new SettingsWindow();
        win.ShowDialog();
        // Reload everything in case the resources path or layout changed
        App.Loader.Load(ConfigPath);
        _gods   = App.Loader.LoadGodList();
        _layout = App.Loader.LoadLayout();
        _display?.ApplyLayout(_layout, App.Loader.Config);
        _overlay?.ApplyLayout(_layout, App.Loader.Config);
        BuildRowViewModels();
        RefreshDisplay();
    }

    private void BtnShowDisplay_Click(object sender, RoutedEventArgs e)
    {
        if (_display is null) return;
        if (_display.WindowState == WindowState.Minimized)
            _display.WindowState = WindowState.Normal;
        _display.Activate();
    }

    private void BtnShowOverlay_Click(object sender, RoutedEventArgs e)
    {
        if (_overlay is null) return;
        if (_overlay.WindowState == WindowState.Minimized)
            _overlay.WindowState = WindowState.Normal;
        _overlay.Activate();
    }

    private void BtnAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "SmitePnB\nSmite 2 Pick & Ban broadcasting tool\n\n" +
            "Remade by diese\nBuilt on the foundation of the Smite esports community\n\n" +
            "github.com/diese-tech/legacy-app-audit",
            "About SmitePnB", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void RefreshDisplay()
    {
        if (_suppressRefresh) return;
        var showNames = ChkShowGodNames.IsChecked == true;
        _display?.Refresh(_state, _teamOne, _teamTwo, showNames);
        _overlay?.Refresh(_state, _teamOne, _teamTwo, showNames);
    }

    private void Autosave() => App.State.Autosave(_state);

    private void RestoreState(DraftState saved)
    {
        _state = saved;
        if (!string.IsNullOrEmpty(saved.TeamOneFolderName) && CmbTeamOne.Items.Contains(saved.TeamOneFolderName))
            CmbTeamOne.SelectedItem = saved.TeamOneFolderName;
        if (!string.IsNullOrEmpty(saved.TeamTwoFolderName) && CmbTeamTwo.Items.Contains(saved.TeamTwoFolderName))
            CmbTeamTwo.SelectedItem = saved.TeamTwoFolderName;

        TxtLeftScore.Text  = saved.TeamOneScore.ToString();
        TxtRightScore.Text = saved.TeamTwoScore.ToString();

        _suppressRefresh = true;
        BuildRowViewModels();
        for (int i = 0; i < 5; i++)
        {
            _t1Bans[i].GodName   = saved.TeamOneBans[i].GodName;
            _t1Bans[i].IsHovered = saved.TeamOneBans[i].IsHovered;
            _t1Bans[i].IsLocked  = saved.TeamOneBans[i].IsLocked;

            _t2Bans[i].GodName   = saved.TeamTwoBans[i].GodName;
            _t2Bans[i].IsHovered = saved.TeamTwoBans[i].IsHovered;
            _t2Bans[i].IsLocked  = saved.TeamTwoBans[i].IsLocked;

            _t1Picks[i].GodName  = saved.TeamOnePicks[i].GodName;
            _t1Picks[i].IsLocked = saved.TeamOnePicks[i].IsLocked;

            _t2Picks[i].GodName  = saved.TeamTwoPicks[i].GodName;
            _t2Picks[i].IsLocked = saved.TeamTwoPicks[i].IsLocked;
        }
        _suppressRefresh = false;
        RefreshDisplay();
    }
}

// ── Row view models ───────────────────────────────────────────────────────

public class BanRowVm
{
    public string?       Label     { get; set; }
    public List<string>  Gods      { get; set; } = [];
    public string?       GodName   { get; set; }
    public bool          IsHovered { get; set; }
    public bool          IsLocked  { get; set; }
    public BanSlot       Slot      { get; set; } = new();
}

public class PickRowVm
{
    public string?       Role     { get; set; }
    public List<string>  Gods     { get; set; } = [];
    public string?       GodName  { get; set; }
    public bool          IsLocked { get; set; }
    public PickSlot      Slot     { get; set; } = new();
}
