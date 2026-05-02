using SmitePnB.Models;
using Xunit;

namespace SmitePnB.Tests.Models;

public class TeamConfigTests
{
    // ── RecordGame ────────────────────────────────────────────────────────

    [Fact]
    public void RecordGame_IncrementsTotalGamesByOne()
    {
        var team = new TeamConfig { TotalGames = 3 };
        team.RecordGame(EmptyBans());
        Assert.Equal(4, team.TotalGames);
    }

    [Fact]
    public void RecordGame_IncrementsCountForLockedBans()
    {
        var team = new TeamConfig { BanCounts = { ["Achilles"] = 0, ["Agni"] = 0 } };
        var bans = new BanSlot[]
        {
            new() { GodName = "Achilles", IsLocked = true },
            new() { GodName = "Agni",     IsLocked = true },
            new(), new(), new()
        };

        team.RecordGame(bans);

        Assert.Equal(1, team.BanCounts["Achilles"]);
        Assert.Equal(1, team.BanCounts["Agni"]);
    }

    [Fact]
    public void RecordGame_IgnoresUnlockedBans()
    {
        var team = new TeamConfig { BanCounts = { ["Achilles"] = 0 } };
        var bans = new BanSlot[]
        {
            new() { GodName = "Achilles", IsLocked = false },
            new(), new(), new(), new()
        };

        team.RecordGame(bans);

        Assert.Equal(0, team.BanCounts["Achilles"]);
    }

    [Fact]
    public void RecordGame_IgnoresEmptyGodName()
    {
        var team = new TeamConfig { TotalGames = 0 };
        var bans = new BanSlot[]
        {
            new() { GodName = string.Empty, IsLocked = true },
            new(), new(), new(), new()
        };

        // Should not throw and should still increment TotalGames
        team.RecordGame(bans);
        Assert.Equal(1, team.TotalGames);
    }

    [Fact]
    public void RecordGame_SkipsGodNotInBanCounts()
    {
        // A newly shipped god won't be in older BanData.json files yet
        var team = new TeamConfig { BanCounts = { ["Achilles"] = 0 } };
        var bans = new BanSlot[]
        {
            new() { GodName = "BrandNewGod", IsLocked = true },
            new(), new(), new(), new()
        };

        var ex = Record.Exception(() => team.RecordGame(bans));
        Assert.Null(ex); // must not throw
    }

    // ── GetTopBans ────────────────────────────────────────────────────────

    [Fact]
    public void GetTopBans_ReturnsSortedDescending()
    {
        var team = new TeamConfig
        {
            BanCounts = { ["Achilles"] = 3, ["Agni"] = 7, ["Ares"] = 1, ["Janus"] = 5 }
        };

        var top = team.GetTopBans(4);

        Assert.Equal("Agni",     top[0].GodName);
        Assert.Equal("Janus",    top[1].GodName);
        Assert.Equal("Achilles", top[2].GodName);
        Assert.Equal("Ares",     top[3].GodName);
    }

    [Fact]
    public void GetTopBans_RespectsNLimit()
    {
        var team = new TeamConfig
        {
            BanCounts = { ["A"] = 5, ["B"] = 4, ["C"] = 3, ["D"] = 2 }
        };

        Assert.Equal(2, team.GetTopBans(2).Count);
    }

    [Fact]
    public void GetTopBans_ReturnsAllWhenNExceedsCount()
    {
        var team = new TeamConfig
        {
            BanCounts = { ["A"] = 5, ["B"] = 4 }
        };

        Assert.Equal(2, team.GetTopBans(10).Count);
    }

    [Fact]
    public void GetTopBans_ReturnsEmptyForNoBans()
    {
        var team = new TeamConfig();
        Assert.Empty(team.GetTopBans(6));
    }

    [Fact]
    public void GetTopBans_DefaultsToSix()
    {
        var team = new TeamConfig();
        for (int i = 0; i < 10; i++)
            team.BanCounts[$"God{i}"] = i;

        Assert.Equal(6, team.GetTopBans().Count);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static BanSlot[] EmptyBans() =>
        Enumerable.Range(0, 5).Select(_ => new BanSlot()).ToArray();
}
