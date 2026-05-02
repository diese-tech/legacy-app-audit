# SmitePnB — Smite 2 Pick & Ban Broadcasting Suite

A standalone Windows desktop app and OBS scene collection for running live
pick-and-ban overlays during competitive Smite 2 broadcasts.

Remade by diese. Built on the foundation of the Smite esports community.

---

## What it does

SmitePnB gives the broadcast operator a control panel to track picks and bans
as they happen in-game. The app runs a separate display window that OBS
captures and puts on stream. The operator controls everything — when bans
appear, when picks lock in, when audio callouts play. Nothing is automated.

---

## Download

**[→ Latest release](../../releases/latest)**

Download `SmitePnB.zip` from the Releases page. Extract it — you'll get `SmitePnB.exe` and a `Resources/` folder side by side. No installer, no runtime.

---

## What you need

- Windows 10 or 11
- OBS Studio

---

## Setup Guide

### Step 1 — Set your resources path

On first launch, SmitePnB will ask you to point it at the Resources folder.
That folder contains everything the app needs: god images, sounds, and team data.

If you extracted the release zip, that path will be something like:
```
C:\SmitePnB\Resources
```

If you're running from the repo, it'll be something like:
```
C:\Projects\legacy-app-audit\SmitePnB\Resources
```

Set it in the Settings window (⚙ button in the top bar) and hit Save.

### Step 2 — Import the OBS scene collection

In OBS: **Scene Collection → Import → select `SmitePnB_OBS.json`**

This gives you six ready-to-use scenes:

| Scene | When to use |
|---|---|
| STARTING SOON | Pre-show, waiting for viewers |
| BRB | Short breaks between games |
| DESK | Analyst desk, between matches |
| PNBS | Live pick and ban — switch here before draft starts |
| GAME | In-game capture |
| POST GAME | End of match, results screen |

The pick-and-ban overlay in the PNBS scene captures the SmitePnB display
window automatically. As long as the app is running, OBS will pick it up.

**Replacing the background images and logos:**
The scene collection references asset files under `C:/SmitePnB/Assets/`.
These are placeholder paths — the original backgrounds and logos were
league-specific and aren't included here. In OBS, open each scene and update
the Image/Media sources to point to your own league's artwork. Everything else
(the pick/ban overlay, scene structure, transitions) works out of the box.

### Step 3 — Add your teams

Each team gets a folder inside `Resources/Teams/`. The folder name is what
appears in the team dropdown.

Inside each folder:
- **`Roster.txt`** — one player name per line, in role order: Solo, Jungle, Mid, Support, Carry. Use `-` for an empty slot.
- **`BanData.json`** — tracks historical ban counts. The app updates this automatically when you submit ban data after a game.

Blank templates are already included. To add a new team, copy a blank folder,
rename it to the team name, and fill in the roster.

---

## Operator Usage Guide

### Before the match

1. Launch **SmitePnB.exe**
2. Select **Team One** (left side) and **Team Two** (right side) from the dropdowns
3. Switch OBS to the **PNBS** scene
4. You're ready

### During the draft

**Bans:**
Each team has 5 ban slots. For each ban:
- Pick the god from the dropdown
- Check **Hover** to show the ban on stream before it's confirmed — useful for building suspense or showing what players are considering
- Check **Lock** to commit the ban to the display and play the audio callout

**Picks:**
Each team has 5 pick slots labeled by role. For each pick:
- Pick the god from the dropdown
- Check **Lock** to commit the pick to the display and play the audio callout

**Scores:**
Update the score boxes at the top as games in the series are won.

**God names toggle:**
The **God Names** checkbox in the top bar shows or hides the god's name as text
on top of their pick image. Turn it off if you prefer a cleaner look.

### After the game

Click **Submit Bans** to record this game's ban data to the team files.
This updates the historical ban counts used in the top-bans display.
You'll get a confirmation prompt — it will tell you how many bans are being recorded.

Then click **New Draft** to clear the board for the next game.

### Swapping sides

If teams need to swap left/right sides between games, hit **⇄ Swap Teams**
in the top bar. The dropdowns will flip and the display updates immediately.

### If something goes wrong

If the app crashes or needs a restart mid-draft, just relaunch it.
You'll be asked if you want to restore the last saved state — say Yes and
everything comes back exactly as it was, including hover and lock states.

---

## Customising the display

### Moving things around

Open **Settings → "Open layout.json in default editor"**.

The file is plain JSON with named positions — every element on the display
window has an `x` and `y` value you can change. Save the file and relaunch
or hit Save in Settings to apply.

If you make a mess of it, **Settings → "Reset layout to defaults"** puts
everything back.

### Fonts and colors

In the **Settings** window:
- Pick any font installed on your machine from the Font dropdown
- Set hex color codes for god names, team names, and score text
- Color swatches update live so you can see what you're getting

### Role labels

Also in Settings, under **Role Labels** — change these if your league uses
different role names. Five entries, one per line.

---

## Adding new gods

When a new god ships in Smite 2:

1. Add the god's name to `Resources/CharactersList.txt` (one name per line, alphabetical order is just convention)
2. Drop their images into `Resources/CharacterImages/Picks/`, `Bans/`, and `TopBans/` — filename must match the name in CharactersList.txt exactly, e.g. `New God.png`
3. Drop their audio callout into `Resources/Sounds/` — e.g. `New God.mp3`

No restart or reinstall needed — the app reads the files fresh each launch.

---

## Project layout

```
SmitePnB/
├── SmitePnB.sln                 Solution file — open this in Visual Studio
├── SmitePnB/                    Application source code
│   ├── Models/                  Data structures (draft state, team config, layout)
│   ├── Services/                Resource loading, autosave, audio
│   └── Windows/                 UI windows (operator panel, display, settings)
└── Resources/                   Everything the app reads at runtime
    ├── CharactersList.txt        Active god roster
    ├── CharacterImages/          Pick, Ban, and TopBan images
    ├── Sounds/                   Per-god audio callouts + hover/lockin SFX
    ├── Teams/                    One folder per team
    └── Display/
        └── layout.json           Element positions on the display window
```

### Building a release

```
cd SmitePnB
dotnet publish SmitePnB/SmitePnB.csproj -c Release
```

Output lands in `SmitePnB/bin/Release/net8.0-windows/win-x64/publish/`.
The result is a single `SmitePnB.exe` with no external dependencies — copy it
and the `Resources/` folder to any Windows 10/11 machine and it runs.

---

## Testing

### Running the tests

From the `SmitePnB/` folder (the one containing the `.sln` file):

```
dotnet test
```

That's it. No setup, no database, no external dependencies. The test suite spins up its own temp folders, runs, and cleans up after itself.

### What is being tested and why

| Test group | Tests | Why these fields |
|---|---|---|
| **DraftState** | `Clear()` resets team names, scores, folder names, all ban slots, all pick slots | The autosave restore flow depends on `Clear()` being complete — a half-cleared state mid-broadcast would silently carry over old picks into a new draft |
| **BanSlot / PickSlot** | `Clear()` resets god name, hover, and lock individually | Each field drives a different visual on the display window; one field left set means a ghost ban or pick showing on stream |
| **TeamConfig — RecordGame** | Only locked bans count; unlocked bans are ignored; empty god names are skipped; unknown gods don't throw | Ban stats are permanent — a double-count or a crash on submit can't be undone without manually editing JSON files |
| **TeamConfig — GetTopBans** | Sorted descending; N limit respected; empty roster; N larger than available bans | The top-bans display reads this directly — wrong order or wrong count shows the wrong gods on stream |
| **StateSerializer round-trip** | Team names, scores, ban hover/lock state, pick lock state all survive save → reload | If any field is dropped during serialization, the operator's restore after a crash is incomplete and they'd have to re-enter the draft from memory mid-event |
| **StateSerializer expiry** | Saves older than 12 hours are rejected; saves within 12 hours are accepted | A stale save from a previous event day appearing as a restore prompt on the next event day would confuse the operator |
| **StateSerializer corruption** | Corrupt JSON returns null and deletes the file | A corrupt autosave that crashes the restore prompt crashes the whole startup — the app must degrade gracefully |
| **ResourceLoader — VerifyResources** | Missing subfolders and missing CharactersList.txt each produce a specific error message | This is the gatekeeper that runs on startup — vague or missing errors leave the operator unable to diagnose what's wrong with their Resources folder |
| **ResourceLoader — LoadGodList** | Blank lines are skipped; missing file returns empty list | Blank lines in CharactersList.txt become empty entries in every god dropdown; a missing file that throws would crash startup |
| **ResourceLoader — TryAddGod** | Blank name rejected; duplicate name rejected case-insensitively; each asset type copied to the correct folder; sound extension preserved; works with no assets at all | Files copied to the wrong folder are silently missing on stream; case-insensitive duplicate check prevents `Achilles` and `achilles` coexisting in the dropdown |
| **ResourceLoader — RemoveGod** | Removed from list; other gods unaffected; asset files kept on disk; missing CharactersList.txt doesn't throw | Asset files are kept so a god can be re-added without re-importing — deleting them would be unrecoverable |
| **ResourceLoader — GetAllGodAssetStatus** | Detects `.png`, `.mp3`, and `.wav` presence; all missing by default | The God Manager status grid reads this — a false positive hides a missing asset that will show as a blank slot on stream |
| **ResourceLoader — LoadTeam** | Corrupt BanData.json falls back to folder name; missing folder returns empty config | A team file that throws on load would crash the entire operator panel at startup |
| **ResourceLoader — GetGodSoundPath** | Finds `.mp3`; finds `.wav`; returns null when neither exists | God callout audio checks both extensions — only checking `.mp3` silently drops any `.wav` callouts |

### What passing tests mean

The core data layer is sound: draft state serializes and deserializes correctly, ban stats can't be double-counted or crash on bad input, god files land in the right folders, and every fail-soft path (corrupt files, missing files, bad JSON) returns a safe value instead of an exception. A broadcast won't crash due to any of the logic covered here.

The UI windows (operator panel, display, settings) and audio playback are not covered by automated tests — those are exercised manually before each event.

### Successful test output

When all tests pass you will see:

```
Passed!  - Failed:     0, Passed:    51, Skipped:     0, Total:    51
```

If `Failed` is anything other than `0`, do not use the build for a live broadcast until the failures are investigated.

---

## Contributing

Pull requests welcome. If you're adding gods, fixing assets, or improving the
display — just fork, make your changes, and open a PR. The goal is for this
to be community-maintained so it doesn't disappear when one person moves on.
