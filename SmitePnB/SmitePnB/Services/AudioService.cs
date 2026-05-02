using System.Windows.Media;

namespace SmitePnB.Services;

/// <summary>
/// Plays hover, lock-in, and per-god audio callouts.
/// Uses WPF MediaPlayer — no external dependencies, no browser, minimal CPU cost.
/// Failures are swallowed so a missing sound file never crashes a live broadcast.
/// </summary>
public class AudioService : IDisposable
{
    private readonly MediaPlayer _player = new();
    private bool _disposed;

    /// <summary>Plays the hover sound when an operator mouses over a god.</summary>
    public void PlayHover(string? path) => Play(path);

    /// <summary>Plays the lock-in thud then the god's name callout.</summary>
    public void PlayLockIn(string? lockInPath, string? godCalloutPath)
    {
        // Lock-in sfx first; god callout fires when it ends
        if (!string.IsNullOrEmpty(lockInPath))
        {
            Play(lockInPath);
            if (!string.IsNullOrEmpty(godCalloutPath))
            {
                _player.MediaEnded -= OnLockInEnded;
                _player.MediaEnded += OnLockInEnded;
                _pendingCallout = godCalloutPath;
            }
        }
        else if (!string.IsNullOrEmpty(godCalloutPath))
        {
            Play(godCalloutPath);
        }
    }

    private string? _pendingCallout;

    private void OnLockInEnded(object? sender, EventArgs e)
    {
        _player.MediaEnded -= OnLockInEnded;
        if (_pendingCallout is not null)
        {
            Play(_pendingCallout);
            _pendingCallout = null;
        }
    }

    private void Play(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            _player.Stop();
            _player.Open(new Uri(path, UriKind.Absolute));
            _player.Play();
        }
        catch { /* missing or corrupt file — don't crash */ }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _player.Stop();
        _player.Close();
    }
}
