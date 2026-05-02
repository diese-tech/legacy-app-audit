using SmitePnB.Models;
using SmitePnB.Services;
using Xunit;

namespace SmitePnB.Tests.Services;

// All tests in this class share the same autosave file path (next to the test binary)
// so they must not run in parallel with each other.
[Collection("StateSerializer")]
public class StateSerializerTests : IDisposable
{
    private readonly StateSerializer _serializer = new();

    public void Dispose() => _serializer.DeleteAutosave();

    // ── Round-trip ────────────────────────────────────────────────────────

    [Fact]
    public void Autosave_ThenRestore_PreservesTeamNames()
    {
        var state = new DraftState { TeamOneName = "Team A", TeamTwoName = "Team B" };
        _serializer.Autosave(state);

        var restored = _serializer.TryLoadAutosave();

        Assert.NotNull(restored);
        Assert.Equal("Team A", restored.TeamOneName);
        Assert.Equal("Team B", restored.TeamTwoName);
    }

    [Fact]
    public void Autosave_ThenRestore_PreservesScores()
    {
        var state = new DraftState { TeamOneScore = 2, TeamTwoScore = 1 };
        _serializer.Autosave(state);

        var restored = _serializer.TryLoadAutosave();

        Assert.NotNull(restored);
        Assert.Equal(2, restored.TeamOneScore);
        Assert.Equal(1, restored.TeamTwoScore);
    }

    [Fact]
    public void Autosave_ThenRestore_PreservesBanSlotState()
    {
        var state = new DraftState();
        state.TeamOneBans[0].GodName   = "Achilles";
        state.TeamOneBans[0].IsHovered = true;
        state.TeamOneBans[0].IsLocked  = true;
        state.TeamTwoBans[4].GodName   = "Agni";
        state.TeamTwoBans[4].IsLocked  = true;

        _serializer.Autosave(state);
        var restored = _serializer.TryLoadAutosave();

        Assert.NotNull(restored);
        Assert.Equal("Achilles", restored.TeamOneBans[0].GodName);
        Assert.True(restored.TeamOneBans[0].IsHovered);
        Assert.True(restored.TeamOneBans[0].IsLocked);
        Assert.Equal("Agni", restored.TeamTwoBans[4].GodName);
        Assert.True(restored.TeamTwoBans[4].IsLocked);
    }

    [Fact]
    public void Autosave_ThenRestore_PreservesPickSlotState()
    {
        var state = new DraftState();
        state.TeamOnePicks[2].GodName  = "Janus";
        state.TeamOnePicks[2].IsLocked  = true;

        _serializer.Autosave(state);
        var restored = _serializer.TryLoadAutosave();

        Assert.NotNull(restored);
        Assert.Equal("Janus", restored.TeamOnePicks[2].GodName);
        Assert.True(restored.TeamOnePicks[2].IsLocked);
    }

    [Fact]
    public void Autosave_SetsCurrentTimestamp()
    {
        var before = DateTime.Now.AddSeconds(-1);
        _serializer.Autosave(new DraftState());
        var after  = DateTime.Now.AddSeconds(1);

        var restored = _serializer.TryLoadAutosave();

        Assert.NotNull(restored);
        Assert.InRange(restored.SavedAt, before, after);
    }

    // ── Missing / deleted ─────────────────────────────────────────────────

    [Fact]
    public void TryLoadAutosave_WhenNoFile_ReturnsNull()
    {
        _serializer.DeleteAutosave();
        Assert.Null(_serializer.TryLoadAutosave());
    }

    [Fact]
    public void DeleteAutosave_RemovesFile_SoNextLoadReturnsNull()
    {
        _serializer.Autosave(new DraftState());
        Assert.True(_serializer.HasAutosave);

        _serializer.DeleteAutosave();

        Assert.False(_serializer.HasAutosave);
        Assert.Null(_serializer.TryLoadAutosave());
    }

    // ── Corrupt file ──────────────────────────────────────────────────────

    [Fact]
    public void TryLoadAutosave_WithCorruptJson_ReturnsNull()
    {
        // Write garbage directly to the autosave path
        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "draft_autosave.json");
        System.IO.File.WriteAllText(path, "{ this is not valid json {{{{");

        var result = _serializer.TryLoadAutosave();

        Assert.Null(result);
        // File should be cleaned up after a failed load
        Assert.False(_serializer.HasAutosave);
    }

    // ── Expiry ────────────────────────────────────────────────────────────

    [Fact]
    public void TryLoadAutosave_WhenOlderThan12Hours_ReturnsNull()
    {
        // Manually write a save with a stale timestamp
        var staleState = new DraftState { SavedAt = DateTime.Now.AddHours(-13) };
        var json = System.Text.Json.JsonSerializer.Serialize(staleState,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "draft_autosave.json");
        System.IO.File.WriteAllText(path, json);

        var result = _serializer.TryLoadAutosave();

        Assert.Null(result);
    }

    [Fact]
    public void TryLoadAutosave_WhenWithin12Hours_ReturnsState()
    {
        var recentState = new DraftState
        {
            TeamOneName = "Recent Team",
            SavedAt     = DateTime.Now.AddHours(-11)
        };
        var json = System.Text.Json.JsonSerializer.Serialize(recentState,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "draft_autosave.json");
        System.IO.File.WriteAllText(path, json);

        var result = _serializer.TryLoadAutosave();

        Assert.NotNull(result);
        Assert.Equal("Recent Team", result.TeamOneName);
    }
}
