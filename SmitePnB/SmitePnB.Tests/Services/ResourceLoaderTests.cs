using System.Text.Json;
using SmitePnB.Models;
using SmitePnB.Services;
using Xunit;

namespace SmitePnB.Tests.Services;

/// <summary>
/// Integration tests for ResourceLoader using a real temp directory so the
/// file-system interactions are exercised exactly as they are in production.
/// Each test class instance gets a fresh temp folder; Dispose cleans it up.
/// </summary>
public class ResourceLoaderTests : IDisposable
{
    private readonly string _res;   // temp Resources folder
    private readonly string _cfg;   // temp config.json path
    private readonly ResourceLoader _loader;

    public ResourceLoaderTests()
    {
        _res = Path.Combine(Path.GetTempPath(), "SmitePnBTest_" + Guid.NewGuid().ToString("N"));
        _cfg = Path.GetTempFileName();
        CreateValidStructure();

        _loader = new ResourceLoader();
        WriteConfig(new AppConfig { ResourcesPath = _res });
        _loader.Load(_cfg);
    }

    public void Dispose()
    {
        if (Directory.Exists(_res)) Directory.Delete(_res, recursive: true);
        if (File.Exists(_cfg))      File.Delete(_cfg);
    }

    // ── VerifyResources ───────────────────────────────────────────────────

    [Fact]
    public void VerifyResources_WithValidStructure_ReturnsNoProblems()
    {
        Assert.Empty(_loader.VerifyResources());
    }

    [Fact]
    public void VerifyResources_WithMissingSubfolder_ReturnsProblem()
    {
        Directory.Delete(Path.Combine(_res, "Sounds"), recursive: true);
        var problems = _loader.VerifyResources();
        Assert.Contains(problems, p => p.Contains("Sounds"));
    }

    [Fact]
    public void VerifyResources_WithMissingCharactersList_ReturnsProblem()
    {
        File.Delete(Path.Combine(_res, "CharactersList.txt"));
        var problems = _loader.VerifyResources();
        Assert.Contains(problems, p => p.Contains("CharactersList.txt"));
    }

    [Fact]
    public void VerifyResources_WithBlankPath_ReturnsProblem()
    {
        WriteConfig(new AppConfig { ResourcesPath = string.Empty });
        _loader.Load(_cfg);
        Assert.NotEmpty(_loader.VerifyResources());
    }

    // ── LoadGodList ───────────────────────────────────────────────────────

    [Fact]
    public void LoadGodList_ReturnsGodsInOrder()
    {
        var gods = _loader.LoadGodList();
        Assert.Equal(new[] { "Achilles", "Agni", "Ares" }, gods);
    }

    [Fact]
    public void LoadGodList_SkipsBlankLines()
    {
        File.WriteAllText(Path.Combine(_res, "CharactersList.txt"), "Achilles\n\n  \nAres\n");
        var gods = _loader.LoadGodList();
        Assert.Equal(2, gods.Count);
    }

    [Fact]
    public void LoadGodList_WhenFileMissing_ReturnsEmpty()
    {
        File.Delete(Path.Combine(_res, "CharactersList.txt"));
        Assert.Empty(_loader.LoadGodList());
    }

    // ── TryAddGod ─────────────────────────────────────────────────────────

    [Fact]
    public void TryAddGod_WithBlankName_ReturnsFalse()
    {
        Assert.False(_loader.TryAddGod("   ", null, null, null, null, out var error));
        Assert.NotEmpty(error);
    }

    [Fact]
    public void TryAddGod_WithDuplicateName_ReturnsFalse()
    {
        Assert.False(_loader.TryAddGod("Achilles", null, null, null, null, out var error));
        Assert.Contains("already exists", error);
    }

    [Fact]
    public void TryAddGod_WithDuplicateName_CaseInsensitive()
    {
        Assert.False(_loader.TryAddGod("achilles", null, null, null, null, out _));
    }

    [Fact]
    public void TryAddGod_AppendsNameToCharactersList()
    {
        _loader.TryAddGod("Bacchus", null, null, null, null, out _);
        Assert.Contains("Bacchus", _loader.LoadGodList());
    }

    [Fact]
    public void TryAddGod_DoesNotDuplicateExistingEntries()
    {
        _loader.TryAddGod("Bacchus", null, null, null, null, out _);
        var gods = _loader.LoadGodList();
        Assert.Equal(gods.Count, gods.Distinct().Count());
    }

    [Fact]
    public void TryAddGod_CopiesPickImageToCorrectFolder()
    {
        var src = WriteTempFile(".png");
        _loader.TryAddGod("Bacchus", pickPath: src, null, null, null, out _);
        Assert.True(File.Exists(Path.Combine(_res, "CharacterImages", "Picks", "Bacchus.png")));
    }

    [Fact]
    public void TryAddGod_CopiesBanImageToCorrectFolder()
    {
        var src = WriteTempFile(".png");
        _loader.TryAddGod("Bacchus", null, banPath: src, null, null, out _);
        Assert.True(File.Exists(Path.Combine(_res, "CharacterImages", "Bans", "Bacchus.png")));
    }

    [Fact]
    public void TryAddGod_CopiesTopBanImageToCorrectFolder()
    {
        var src = WriteTempFile(".png");
        _loader.TryAddGod("Bacchus", null, null, topBanPath: src, null, out _);
        Assert.True(File.Exists(Path.Combine(_res, "CharacterImages", "TopBans", "Bacchus.png")));
    }

    [Fact]
    public void TryAddGod_CopiesSoundPreservingExtension()
    {
        var src = WriteTempFile(".mp3");
        _loader.TryAddGod("Bacchus", null, null, null, soundPath: src, out _);
        Assert.True(File.Exists(Path.Combine(_res, "Sounds", "Bacchus.mp3")));
    }

    [Fact]
    public void TryAddGod_WithNoAssets_StillAddsToList()
    {
        var ok = _loader.TryAddGod("Bacchus", null, null, null, null, out _);
        Assert.True(ok);
        Assert.Contains("Bacchus", _loader.LoadGodList());
    }

    // ── RemoveGod ─────────────────────────────────────────────────────────

    [Fact]
    public void RemoveGod_RemovesFromCharactersList()
    {
        _loader.RemoveGod("Agni");
        Assert.DoesNotContain("Agni", _loader.LoadGodList());
    }

    [Fact]
    public void RemoveGod_LeavesOtherGodsIntact()
    {
        _loader.RemoveGod("Agni");
        var gods = _loader.LoadGodList();
        Assert.Contains("Achilles", gods);
        Assert.Contains("Ares",     gods);
    }

    [Fact]
    public void RemoveGod_DoesNotDeleteAssetFiles()
    {
        var imgPath = Path.Combine(_res, "CharacterImages", "Picks", "Achilles.png");
        File.WriteAllBytes(imgPath, new byte[] { 0x89, 0x50 });

        _loader.RemoveGod("Achilles");

        Assert.True(File.Exists(imgPath));
    }

    [Fact]
    public void RemoveGod_WhenMissingFile_DoesNotThrow()
    {
        File.Delete(Path.Combine(_res, "CharactersList.txt"));
        var ex = Record.Exception(() => _loader.RemoveGod("Achilles"));
        Assert.Null(ex);
    }

    // ── GetAllGodAssetStatus ──────────────────────────────────────────────

    [Fact]
    public void GetAllGodAssetStatus_DetectsPresent_PickImage()
    {
        File.WriteAllBytes(Path.Combine(_res, "CharacterImages", "Picks", "Achilles.png"), new byte[] { 1 });
        var status = StatusFor("Achilles");
        Assert.True(status.HasPick);
        Assert.False(status.HasBan);
    }

    [Fact]
    public void GetAllGodAssetStatus_DetectsPresent_BanImage()
    {
        File.WriteAllBytes(Path.Combine(_res, "CharacterImages", "Bans", "Agni.png"), new byte[] { 1 });
        var status = StatusFor("Agni");
        Assert.True(status.HasBan);
    }

    [Fact]
    public void GetAllGodAssetStatus_DetectsPresent_TopBanImage()
    {
        File.WriteAllBytes(Path.Combine(_res, "CharacterImages", "TopBans", "Ares.png"), new byte[] { 1 });
        var status = StatusFor("Ares");
        Assert.True(status.HasTopBan);
    }

    [Fact]
    public void GetAllGodAssetStatus_DetectsPresent_Mp3Sound()
    {
        File.WriteAllBytes(Path.Combine(_res, "Sounds", "Achilles.mp3"), new byte[] { 1 });
        Assert.True(StatusFor("Achilles").HasSound);
    }

    [Fact]
    public void GetAllGodAssetStatus_DetectsPresent_WavSound()
    {
        File.WriteAllBytes(Path.Combine(_res, "Sounds", "Achilles.wav"), new byte[] { 1 });
        Assert.True(StatusFor("Achilles").HasSound);
    }

    [Fact]
    public void GetAllGodAssetStatus_AllMissingByDefault()
    {
        var status = StatusFor("Achilles");
        Assert.False(status.HasPick);
        Assert.False(status.HasBan);
        Assert.False(status.HasTopBan);
        Assert.False(status.HasSound);
    }

    // ── LoadTeam ──────────────────────────────────────────────────────────

    [Fact]
    public void LoadTeam_WithCorruptBanData_ReturnsFallback()
    {
        var teamDir = Path.Combine(_res, "Teams", "TestTeam");
        Directory.CreateDirectory(teamDir);
        File.WriteAllText(Path.Combine(teamDir, "BanData.json"), "{ not valid json {{{{");

        var team = _loader.LoadTeam("TestTeam");

        Assert.Equal("TestTeam", team.TeamName); // falls back to folder name
        Assert.Equal(0, team.TotalGames);
    }

    [Fact]
    public void LoadTeam_WithValidBanData_LoadsCorrectly()
    {
        var teamDir = Path.Combine(_res, "Teams", "Raiders");
        Directory.CreateDirectory(teamDir);
        File.WriteAllText(Path.Combine(teamDir, "BanData.json"), """
            {
                "teamname": "The Raiders",
                "totalgames": 12,
                "bancounts": { "Achilles": 5, "Agni": 3 }
            }
            """);

        var team = _loader.LoadTeam("Raiders");

        Assert.Equal("The Raiders", team.TeamName);
        Assert.Equal(12, team.TotalGames);
        Assert.Equal(5, team.BanCounts["Achilles"]);
    }

    [Fact]
    public void LoadTeam_MissingFolder_ReturnsEmptyConfig()
    {
        var team = _loader.LoadTeam("DoesNotExist");
        Assert.Equal("DoesNotExist", team.TeamName);
        Assert.Equal(0, team.TotalGames);
    }

    // ── GetGodSoundPath ───────────────────────────────────────────────────

    [Fact]
    public void GetGodSoundPath_FindsMp3()
    {
        var path = Path.Combine(_res, "Sounds", "Achilles.mp3");
        File.WriteAllBytes(path, new byte[] { 1 });
        Assert.Equal(path, _loader.GetGodSoundPath("Achilles"));
    }

    [Fact]
    public void GetGodSoundPath_FindsWav()
    {
        var path = Path.Combine(_res, "Sounds", "Achilles.wav");
        File.WriteAllBytes(path, new byte[] { 1 });
        Assert.Equal(path, _loader.GetGodSoundPath("Achilles"));
    }

    [Fact]
    public void GetGodSoundPath_WhenMissing_ReturnsNull()
    {
        Assert.Null(_loader.GetGodSoundPath("NoSoundGod"));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void CreateValidStructure()
    {
        foreach (var sub in new[]
        {
            "CharacterImages/Picks", "CharacterImages/Bans", "CharacterImages/TopBans",
            "Sounds", "Teams", "Display"
        })
        {
            Directory.CreateDirectory(Path.Combine(_res, sub));
        }
        File.WriteAllLines(Path.Combine(_res, "CharactersList.txt"), ["Achilles", "Agni", "Ares"]);
    }

    private void WriteConfig(AppConfig config) =>
        File.WriteAllText(_cfg, JsonSerializer.Serialize(config));

    private ResourceLoader.GodAssetStatus StatusFor(string name) =>
        _loader.GetAllGodAssetStatus().First(s => s.Name == name);

    private static string WriteTempFile(string ext)
    {
        var path = Path.GetTempFileName() + ext;
        File.WriteAllBytes(path, new byte[] { 0x00 });
        return path;
    }
}
