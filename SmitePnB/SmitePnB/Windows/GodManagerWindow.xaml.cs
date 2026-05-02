using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using SmitePnB.Services;

namespace SmitePnB.Windows;

public partial class GodManagerWindow : Window
{
    public GodManagerWindow() { InitializeComponent(); }

    private string? _pickPath;
    private string? _banPath;
    private string? _topBanPath;
    private string? _soundPath;

    // Set to true when the operator adds or removes a god so the caller can reload the god list
    public bool GodListChanged { get; private set; }

    private List<GodRowVm> _allRows = [];

    private void Window_Loaded(object sender, RoutedEventArgs e) => RefreshList();

    // ── Add new god ───────────────────────────────────────────────────────

    private void BtnPickImage_Click(object sender, RoutedEventArgs e)
        => BrowseImage(ref _pickPath, LblPickFile);

    private void BtnBanImage_Click(object sender, RoutedEventArgs e)
        => BrowseImage(ref _banPath, LblBanFile);

    private void BtnTopBanImage_Click(object sender, RoutedEventArgs e)
        => BrowseImage(ref _topBanPath, LblTopBanFile);

    private void BtnSoundFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Select sound file",
            Filter = "Audio files (*.mp3;*.wav)|*.mp3;*.wav"
        };
        if (dlg.ShowDialog() == true)
        {
            _soundPath = dlg.FileName;
            LblSoundFile.Text       = System.IO.Path.GetFileName(dlg.FileName);
            LblSoundFile.Foreground = Brushes.White;
        }
    }

    private static void BrowseImage(ref string? path, System.Windows.Controls.TextBlock label)
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Select image file",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp"
        };
        if (dlg.ShowDialog() == true)
        {
            path            = dlg.FileName;
            label.Text       = System.IO.Path.GetFileName(dlg.FileName);
            label.Foreground = Brushes.White;
        }
    }

    private void BtnAddGod_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtNewGodName.Text.Trim();

        if (!App.Loader.TryAddGod(name, _pickPath, _banPath, _topBanPath, _soundPath, out var error))
        {
            TxtAddStatus.Text       = error;
            TxtAddStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50));
            return;
        }

        TxtAddStatus.Text       = $"'{name}' added successfully.";
        TxtAddStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
        GodListChanged = true;

        // Reset the form
        TxtNewGodName.Text   = string.Empty;
        _pickPath = _banPath = _topBanPath = _soundPath = null;
        LblPickFile.Text   = LblBanFile.Text = LblTopBanFile.Text = LblSoundFile.Text = "no file selected";
        LblPickFile.Foreground = LblBanFile.Foreground = LblTopBanFile.Foreground = LblSoundFile.Foreground
            = (Brush)FindResource("TextMuted");

        RefreshList();
    }

    // ── Remove god ────────────────────────────────────────────────────────

    private void BtnRemoveGod_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button btn) return;
        var name = btn.Tag as string;
        if (string.IsNullOrEmpty(name)) return;

        var result = MessageBox.Show(
            $"Remove '{name}' from the god list?\n\nThe image and sound files will be kept on disk — you can re-add this god later without re-importing.",
            "Remove God", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        App.Loader.RemoveGod(name);
        GodListChanged = true;
        RefreshList();
    }

    // ── Search / list ─────────────────────────────────────────────────────

    private void TxtSearch_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        => ApplyFilter();

    private void RefreshList()
    {
        _allRows = App.Loader.GetAllGodAssetStatus()
                             .Select(s => new GodRowVm(s))
                             .ToList();
        TxtGodCount.Text = $"{_allRows.Count} god{(_allRows.Count == 1 ? "" : "s")} in roster";
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = TxtSearch?.Text?.Trim() ?? string.Empty;
        GodList.ItemsSource = string.IsNullOrEmpty(query)
            ? _allRows
            : _allRows.Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    // ── Close ─────────────────────────────────────────────────────────────

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}

// ── View model ────────────────────────────────────────────────────────────

public class GodRowVm
{
    public string Name { get; }

    public Visibility PickOkVisibility      { get; }
    public Visibility PickMissingVisibility { get; }
    public Visibility BanOkVisibility       { get; }
    public Visibility BanMissingVisibility  { get; }
    public Visibility TopBanOkVisibility    { get; }
    public Visibility TopBanMissingVisibility { get; }
    public Visibility SoundOkVisibility     { get; }
    public Visibility SoundMissingVisibility { get; }

    public GodRowVm(ResourceLoader.GodAssetStatus s)
    {
        Name = s.Name;
        PickOkVisibility       = s.HasPick    ? Visibility.Visible : Visibility.Collapsed;
        PickMissingVisibility  = s.HasPick    ? Visibility.Collapsed : Visibility.Visible;
        BanOkVisibility        = s.HasBan     ? Visibility.Visible : Visibility.Collapsed;
        BanMissingVisibility   = s.HasBan     ? Visibility.Collapsed : Visibility.Visible;
        TopBanOkVisibility     = s.HasTopBan  ? Visibility.Visible : Visibility.Collapsed;
        TopBanMissingVisibility = s.HasTopBan ? Visibility.Collapsed : Visibility.Visible;
        SoundOkVisibility      = s.HasSound   ? Visibility.Visible : Visibility.Collapsed;
        SoundMissingVisibility = s.HasSound   ? Visibility.Collapsed : Visibility.Visible;
    }
}
