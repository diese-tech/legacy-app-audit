using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SmitePnB.Models;
using SmitePnB.Services;

namespace SmitePnB.Windows;

public partial class MainWindow : Window
{
    private static readonly string ConfigPath =
        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    public MainWindow() { InitializeComponent(); }

    private DraftState   _state   = new();
    private TeamConfig?  _teamOne;
    private TeamConfig?  _teamTwo;
    private LayoutConfig _layout  = new();
    private List<string> _gods    = [];
    private DisplayWindow?        _display;
    private InGameOverlayWindow?  _overlay;

    private readonly List<PickPairVm>  _pickPairs  = [];
    private readonly List<BanPairVm>   _banPairs   = [];
    private readonly List<PlayerRowVm> _playerRows = [];

    private bool _suppressRefresh;

    // ── Startup ───────────────────────────────────────────────────────────

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        App.Loader.Load(ConfigPath);

        if (string.IsNullOrEmpty(App.Loader.Config.ResourcesPath) ||
            App.Loader.VerifyResources().Count > 0)
        {
            var settings = new SettingsWindow();
            settings.ShowDialog();
            App.Loader.Load(ConfigPath);
        }

        _gods   = App.Loader.LoadGodList();
        _layout = App.Loader.LoadLayout();

        TxtResourcesPath.Text       = App.Loader.Config.ResourcesPath;
        CmbResolution.SelectedIndex = App.Loader.Config.ResolutionIndex;

        PopulateTeamCombos();
        BuildRowViewModels();

        _display = new DisplayWindow();
        _display.ApplyLayout(_layout, App.Loader.Config);
        _display.Show();

        _overlay = new InGameOverlayWindow();
        _overlay.ApplyLayout(_layout, App.Loader.Config);
        _overlay.Show();

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

    // ── Team combos ───────────────────────────────────────────────────────

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
            RefreshPlayerRoster();
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
            RefreshPlayerRoster();
            RefreshDisplay();
        }
    }

    // ── Row view models ───────────────────────────────────────────────────

    private void BuildRowViewModels()
    {
        _pickPairs.Clear();
        _banPairs.Clear();

        var roles = _layout.RoleLabels;
        for (int i = 0; i < 5; i++)
        {
            var t1Pick = _state.TeamOnePicks[i];
            var t2Pick = _state.TeamTwoPicks[i];
            _pickPairs.Add(new PickPairVm
            {
                RowNum   = i + 1,
                Gods     = _gods,
                T1Slot   = t1Pick,
                T2Slot   = t2Pick,
                T1God    = string.IsNullOrEmpty(t1Pick.GodName) ? null : t1Pick.GodName,
                T1Locked = t1Pick.IsLocked,
                T2God    = string.IsNullOrEmpty(t2Pick.GodName) ? null : t2Pick.GodName,
                T2Locked = t2Pick.IsLocked,
            });

            var t1Ban = _state.TeamOneBans[i];
            var t2Ban = _state.TeamTwoBans[i];
            _banPairs.Add(new BanPairVm
            {
                RowNum   = i + 1,
                Gods     = _gods,
                T1Slot   = t1Ban,
                T2Slot   = t2Ban,
                T1God    = string.IsNullOrEmpty(t1Ban.GodName) ? null : t1Ban.GodName,
                T1Locked = t1Ban.IsLocked,
                T2God    = string.IsNullOrEmpty(t2Ban.GodName) ? null : t2Ban.GodName,
                T2Locked = t2Ban.IsLocked,
            });
        }

        PickPairList.ItemsSource = _pickPairs;
        BanPairList.ItemsSource  = _banPairs;
        BuildPlayerRows();
    }

    private void BuildPlayerRows()
    {
        _playerRows.Clear();
        var roles = _layout.RoleLabels;
        for (int i = 0; i < 5; i++)
        {
            _playerRows.Add(new PlayerRowVm
            {
                RowNum   = i + 1,
                Role     = roles[i],
                T1Player = _teamOne?.Roster.ElementAtOrDefault(i) ?? string.Empty,
                T2Player = _teamTwo?.Roster.ElementAtOrDefault(i) ?? string.Empty,
            });
        }
        PlayerRowList.ItemsSource = _playerRows;
    }

    private void RefreshPlayerRoster()
    {
        if (_playerRows.Count == 0) { BuildPlayerRows(); return; }
        for (int i = 0; i < _playerRows.Count; i++)
        {
            _playerRows[i].T1Player = _teamOne?.Roster.ElementAtOrDefault(i) ?? string.Empty;
            _playerRows[i].T2Player = _teamTwo?.Roster.ElementAtOrDefault(i) ?? string.Empty;
        }
        PlayerRowList.ItemsSource = null;
        PlayerRowList.ItemsSource = _playerRows;
    }

    // ── Picks ─────────────────────────────────────────────────────────────

    private void PickT1God_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressRefresh) return;
        if (sender is ComboBox cmb && cmb.Tag is PickPairVm vm)
        {
            vm.T1Slot.GodName = vm.T1God ?? string.Empty;
            App.Audio.PlayHover(App.Loader.GetHoverSoundPath());
        }
        RefreshDisplay();
    }

    private void PickT2God_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressRefresh) return;
        if (sender is ComboBox cmb && cmb.Tag is PickPairVm vm)
        {
            vm.T2Slot.GodName = vm.T2God ?? string.Empty;
            App.Audio.PlayHover(App.Loader.GetHoverSoundPath());
        }
        RefreshDisplay();
    }

    private void PickT1Lock_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox chk && chk.Tag is PickPairVm vm)
        {
            vm.T1Slot.IsLocked = chk.IsChecked == true;
            if (vm.T1Slot.IsLocked)
                App.Audio.PlayLockIn(App.Loader.GetLockInSoundPath(), App.Loader.GetGodSoundPath(vm.T1Slot.GodName));
        }
        Autosave();
        RefreshDisplay();
    }

    private void PickT2Lock_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox chk && chk.Tag is PickPairVm vm)
        {
            vm.T2Slot.IsLocked = chk.IsChecked == true;
            if (vm.T2Slot.IsLocked)
                App.Audio.PlayLockIn(App.Loader.GetLockInSoundPath(), App.Loader.GetGodSoundPath(vm.T2Slot.GodName));
        }
        Autosave();
        RefreshDisplay();
    }

    // ── Bans ──────────────────────────────────────────────────────────────

    private void BanT1God_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressRefresh) return;
        if (sender is ComboBox cmb && cmb.Tag is BanPairVm vm)
        {
            vm.T1Slot.GodName   = vm.T1God ?? string.Empty;
            vm.T1Slot.IsHovered = !string.IsNullOrEmpty(vm.T1Slot.GodName);
            App.Audio.PlayHover(App.Loader.GetHoverSoundPath());
        }
        RefreshDisplay();
    }

    private void BanT2God_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressRefresh) return;
        if (sender is ComboBox cmb && cmb.Tag is BanPairVm vm)
        {
            vm.T2Slot.GodName   = vm.T2God ?? string.Empty;
            vm.T2Slot.IsHovered = !string.IsNullOrEmpty(vm.T2Slot.GodName);
            App.Audio.PlayHover(App.Loader.GetHoverSoundPath());
        }
        RefreshDisplay();
    }

    private void BanT1Lock_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox chk && chk.Tag is BanPairVm vm)
        {
            vm.T1Slot.IsLocked = chk.IsChecked == true;
            if (vm.T1Slot.IsLocked)
                App.Audio.PlayLockIn(App.Loader.GetLockInSoundPath(), App.Loader.GetGodSoundPath(vm.T1Slot.GodName));
        }
        Autosave();
        RefreshDisplay();
    }

    private void BanT2Lock_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox chk && chk.Tag is BanPairVm vm)
        {
            vm.T2Slot.IsLocked = chk.IsChecked == true;
            if (vm.T2Slot.IsLocked)
                App.Audio.PlayLockIn(App.Loader.GetLockInSoundPath(), App.Loader.GetGodSoundPath(vm.T2Slot.GodName));
        }
        Autosave();
        RefreshDisplay();
    }

    // ── Players ───────────────────────────────────────────────────────────

    private void PlayerT1_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox txt && txt.Tag is PlayerRowVm vm && _teamOne is not null)
        {
            var idx = vm.RowNum - 1;
            if (idx >= 0 && idx < _teamOne.Roster.Length)
                _teamOne.Roster[idx] = vm.T1Player ?? string.Empty;
            App.Loader.SaveRoster(_teamOne);
        }
    }

    private void PlayerT2_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox txt && txt.Tag is PlayerRowVm vm && _teamTwo is not null)
        {
            var idx = vm.RowNum - 1;
            if (idx >= 0 && idx < _teamTwo.Roster.Length)
                _teamTwo.Roster[idx] = vm.T2Player ?? string.Empty;
            App.Loader.SaveRoster(_teamTwo);
        }
    }

    // ── Score / god names ─────────────────────────────────────────────────

    private void Score_Changed(object sender, TextChangedEventArgs e)
    {
        if (TxtLeftScore is null || TxtRightScore is null) return;
        if (int.TryParse(TxtLeftScore.Text, out var l)) _state.TeamOneScore = l;
        if (int.TryParse(TxtRightScore.Text, out var r)) _state.TeamTwoScore = r;
        RefreshDisplay();
    }

    private void ChkShowGodNames_Changed(object sender, RoutedEventArgs e)
    {
        if (ChkShowGodNamesVarious is not null)
            ChkShowGodNamesVarious.IsChecked = ChkShowGodNames.IsChecked;
        RefreshDisplay();
    }

    private void ChkShowGodNamesVarious_Changed(object sender, RoutedEventArgs e)
    {
        if (ChkShowGodNames is not null)
            ChkShowGodNames.IsChecked = ChkShowGodNamesVarious.IsChecked;
        RefreshDisplay();
    }

    // ── Inline config ─────────────────────────────────────────────────────

    private void BtnBrowseResources_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog
        {
            Title            = "Select the Resources folder",
            InitialDirectory = TxtResourcesPath.Text
        };
        if (dlg.ShowDialog() != true) return;

        App.Loader.Config.ResourcesPath = dlg.FolderName;
        TxtResourcesPath.Text           = dlg.FolderName;
        App.Loader.SaveConfig(ConfigPath);

        _gods   = App.Loader.LoadGodList();
        _layout = App.Loader.LoadLayout();
        BuildRowViewModels();
        _display?.ApplyLayout(_layout, App.Loader.Config);
        _overlay?.ApplyLayout(_layout, App.Loader.Config);
        RefreshDisplay();
    }

    private void CmbResolution_Changed(object sender, SelectionChangedEventArgs e)
    {
        // Guard: fires during initial construction before _display is set
        if (_display is null) return;
        App.Loader.Config.ResolutionIndex = CmbResolution.SelectedIndex;
        App.Loader.SaveConfig(ConfigPath);
        _display.ApplyLayout(_layout, App.Loader.Config);
        _overlay?.ApplyLayout(_layout, App.Loader.Config);
    }

    // ── Draft actions ─────────────────────────────────────────────────────

    private void BtnNewDraft_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Clear all picks and bans?", "New Draft",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

        _state.Clear();
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

    private void BtnResetPicks_Click(object sender, RoutedEventArgs e)
    {
        foreach (var s in _state.TeamOnePicks) s.Clear();
        foreach (var s in _state.TeamTwoPicks) s.Clear();
        _suppressRefresh = true;
        foreach (var vm in _pickPairs) { vm.T1God = null; vm.T1Locked = false; vm.T2God = null; vm.T2Locked = false; }
        _suppressRefresh = false;
        PickPairList.ItemsSource = null;
        PickPairList.ItemsSource = _pickPairs;
        RefreshDisplay();
    }

    private void BtnResetBans_Click(object sender, RoutedEventArgs e)
    {
        foreach (var s in _state.TeamOneBans) s.Clear();
        foreach (var s in _state.TeamTwoBans) s.Clear();
        _suppressRefresh = true;
        foreach (var vm in _banPairs) { vm.T1God = null; vm.T1Locked = false; vm.T2God = null; vm.T2Locked = false; }
        _suppressRefresh = false;
        BanPairList.ItemsSource = null;
        BanPairList.ItemsSource = _banPairs;
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

        if (MessageBox.Show(msg, "Submit Bans", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        if (_teamOne is not null) { _teamOne.RecordGame(_state.TeamOneBans); App.Loader.SaveTeam(_teamOne); }
        if (_teamTwo is not null) { _teamTwo.RecordGame(_state.TeamTwoBans); App.Loader.SaveTeam(_teamTwo); }

        App.State.DeleteAutosave();
        MessageBox.Show("Ban data saved.", "Submit Bans", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnSwapTeams_Click(object sender, RoutedEventArgs e)
    {
        (CmbTeamOne.SelectedIndex, CmbTeamTwo.SelectedIndex) = (CmbTeamTwo.SelectedIndex, CmbTeamOne.SelectedIndex);
    }

    // ── Window management ─────────────────────────────────────────────────

    private void BtnShowDisplay_Click(object sender, RoutedEventArgs e)
    {
        if (_display is null) return;
        if (_display.WindowState == WindowState.Minimized) _display.WindowState = WindowState.Normal;
        _display.Activate();
    }

    private void BtnShowOverlay_Click(object sender, RoutedEventArgs e)
    {
        if (_overlay is null) return;
        if (_overlay.WindowState == WindowState.Minimized) _overlay.WindowState = WindowState.Normal;
        _overlay.Activate();
    }

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
        App.Loader.Load(ConfigPath);
        _gods   = App.Loader.LoadGodList();
        _layout = App.Loader.LoadLayout();
        TxtResourcesPath.Text       = App.Loader.Config.ResourcesPath;
        CmbResolution.SelectedIndex = App.Loader.Config.ResolutionIndex;
        _display?.ApplyLayout(_layout, App.Loader.Config);
        _overlay?.ApplyLayout(_layout, App.Loader.Config);
        BuildRowViewModels();
        RefreshDisplay();
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
        _suppressRefresh = false;
        RefreshDisplay();
    }
}

// ── View models ───────────────────────────────────────────────────────────

public class PickPairVm
{
    public int          RowNum   { get; set; }
    public List<string> Gods     { get; set; } = [];
    public string?      T1God    { get; set; }
    public bool         T1Locked { get; set; }
    public string?      T2God    { get; set; }
    public bool         T2Locked { get; set; }
    public PickSlot     T1Slot   { get; set; } = new();
    public PickSlot     T2Slot   { get; set; } = new();
}

public class BanPairVm
{
    public int          RowNum   { get; set; }
    public List<string> Gods     { get; set; } = [];
    public string?      T1God    { get; set; }
    public bool         T1Locked { get; set; }
    public string?      T2God    { get; set; }
    public bool         T2Locked { get; set; }
    public BanSlot      T1Slot   { get; set; } = new();
    public BanSlot      T2Slot   { get; set; } = new();
}

public class PlayerRowVm
{
    public int     RowNum   { get; set; }
    public string? Role     { get; set; }
    public string? T1Player { get; set; }
    public string? T2Player { get; set; }
}
