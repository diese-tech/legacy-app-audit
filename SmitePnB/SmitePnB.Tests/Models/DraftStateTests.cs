using SmitePnB.Models;
using Xunit;

namespace SmitePnB.Tests.Models;

public class DraftStateTests
{
    [Fact]
    public void HasFiveSlotsPerTeam()
    {
        var state = new DraftState();
        Assert.Equal(5, state.TeamOneBans.Length);
        Assert.Equal(5, state.TeamTwoBans.Length);
        Assert.Equal(5, state.TeamOnePicks.Length);
        Assert.Equal(5, state.TeamTwoPicks.Length);
    }

    [Fact]
    public void Clear_ResetsTeamNamesAndScores()
    {
        var state = new DraftState
        {
            TeamOneName = "Team A", TeamTwoName = "Team B",
            TeamOneScore = 2,       TeamTwoScore = 1,
            TeamOneFolderName = "folder-a", TeamTwoFolderName = "folder-b"
        };

        state.Clear();

        Assert.Equal(string.Empty, state.TeamOneName);
        Assert.Equal(string.Empty, state.TeamTwoName);
        Assert.Equal(0, state.TeamOneScore);
        Assert.Equal(0, state.TeamTwoScore);
        Assert.Equal(string.Empty, state.TeamOneFolderName);
        Assert.Equal(string.Empty, state.TeamTwoFolderName);
    }

    [Fact]
    public void Clear_ResetsAllBanSlots()
    {
        var state = new DraftState();
        state.TeamOneBans[0].GodName  = "Achilles";
        state.TeamOneBans[0].IsHovered = true;
        state.TeamOneBans[0].IsLocked  = true;
        state.TeamTwoBans[4].GodName  = "Agni";
        state.TeamTwoBans[4].IsLocked  = true;

        state.Clear();

        Assert.All(state.TeamOneBans, b =>
        {
            Assert.Equal(string.Empty, b.GodName);
            Assert.False(b.IsHovered);
            Assert.False(b.IsLocked);
        });
        Assert.All(state.TeamTwoBans, b => Assert.Equal(string.Empty, b.GodName));
    }

    [Fact]
    public void Clear_ResetsAllPickSlots()
    {
        var state = new DraftState();
        state.TeamOnePicks[2].GodName  = "Ares";
        state.TeamOnePicks[2].IsLocked  = true;
        state.TeamTwoPicks[0].GodName  = "Janus";
        state.TeamTwoPicks[0].IsLocked  = true;

        state.Clear();

        Assert.All(state.TeamOnePicks, p =>
        {
            Assert.Equal(string.Empty, p.GodName);
            Assert.False(p.IsLocked);
        });
        Assert.All(state.TeamTwoPicks, p => Assert.Equal(string.Empty, p.GodName));
    }

    [Fact]
    public void BanSlot_Clear_ResetsAllFields()
    {
        var slot = new BanSlot { GodName = "Loki", IsHovered = true, IsLocked = true };
        slot.Clear();
        Assert.Equal(string.Empty, slot.GodName);
        Assert.False(slot.IsHovered);
        Assert.False(slot.IsLocked);
    }

    [Fact]
    public void PickSlot_Clear_ResetsAllFields()
    {
        var slot = new PickSlot { GodName = "Medusa", IsLocked = true };
        slot.Clear();
        Assert.Equal(string.Empty, slot.GodName);
        Assert.False(slot.IsLocked);
    }
}
