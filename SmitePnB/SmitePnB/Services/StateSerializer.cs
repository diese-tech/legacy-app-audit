using System.IO;
using System.Text.Json;
using SmitePnB.Models;

namespace SmitePnB.Services;

/// <summary>
/// Writes the active draft state to disk on every lock-in so a crash or
/// forced restart mid-broadcast can be recovered without re-entering from memory.
///
/// The autosave file lives next to the exe and is deleted on a clean
/// "new draft" or "submit ban data" so the restore prompt never shows
/// for stale data from a previous match.
/// </summary>
public class StateSerializer
{
    private static readonly string AutosavePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "draft_autosave.json");

    private static readonly JsonSerializerOptions Opts =
        new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    // ── Write ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Called after every lock-in. Fire-and-forget — failures are swallowed
    /// so a disk error during a live match never surfaces as a crash.
    /// </summary>
    public void Autosave(DraftState state)
    {
        try
        {
            state.SavedAt = DateTime.Now;
            File.WriteAllText(AutosavePath, JsonSerializer.Serialize(state, Opts));
        }
        catch { /* never crash a live broadcast over a save failure */ }
    }

    public void DeleteAutosave()
    {
        try { if (File.Exists(AutosavePath)) File.Delete(AutosavePath); }
        catch { }
    }

    // ── Read ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a restored DraftState if a valid autosave exists, otherwise null.
    /// Shown to the operator at startup so they can choose to restore or discard.
    /// </summary>
    public DraftState? TryLoadAutosave()
    {
        if (!File.Exists(AutosavePath)) return null;
        try
        {
            var json  = File.ReadAllText(AutosavePath);
            var state = JsonSerializer.Deserialize<DraftState>(json, Opts);
            // Reject saves older than 12 hours — likely from a previous event day
            if (state is null || (DateTime.Now - state.SavedAt).TotalHours > 12)
            {
                DeleteAutosave();
                return null;
            }
            return state;
        }
        catch
        {
            DeleteAutosave();
            return null;
        }
    }

    public bool HasAutosave => File.Exists(AutosavePath);
}
