# SmitePnB - Smite 2 Pick & Ban Broadcasting Suite

A standalone Windows desktop app and OBS scene collection for running live
pick-and-ban overlays during competitive Smite 2 broadcasts.

Remade by diese. Built on the foundation of the Smite esports community.

---

## What it does

SmitePnB gives the broadcast operator a control panel to track picks and bans
as they happen in-game. The app runs a separate display window that OBS
captures and puts on stream. The operator controls everything: when bans
appear, when picks lock in, when audio callouts play. Nothing is automated.

---

## Download

**[Latest release](../../releases/latest)**

Download `SmitePnB.zip` from the Releases page. Extract it and you will get
`SmitePnB.exe` and a `Resources/` folder side by side.

---

## What you need

- Windows 10 or 11
- OBS Studio

---

## Setup Guide

### Step 1 - Set your resources path

On first launch, SmitePnB will use the bundled `Resources` folder automatically
if it sits next to `SmitePnB.exe`. If you move the app or want to use a
different resources folder, open Settings and update the path there.

That folder contains everything the app needs: god images, sounds, and team data.

If you extracted the release zip, that path will be something like:

```text
C:\SmitePnB\Resources
```

If you are running from the repo, it will usually be:

```text
C:\Projects\misc\sal-prod\SmitePnB\Resources
```

Set it in the Settings window and hit Save if the auto-detected path is not
the one you want.

### Step 2 - Import the OBS scene collection

In OBS: **Scene Collection -> Import -> select `SmitePnB_OBS.json`**

This gives you six ready-to-use scenes:

| Scene | When to use |
|---|---|
| STARTING SOON | Pre-show, waiting for viewers |
| BRB | Short breaks between games |
| DESK | Analyst desk, between matches |
| PNBS | Live pick and ban - switch here before draft starts |
| GAME | In-game capture |
| POST GAME | End of match, results screen |

The pick-and-ban overlay in the PNBS scene captures the SmitePnB display
window automatically. As long as the app is running, OBS will pick it up.

**Replacing the background images and logos**

The scene collection references asset files under `C:/SmitePnB/Assets/`.
These are placeholder paths - the original backgrounds and logos were
league-specific and are not included here. In OBS, open each scene and update
the Image/Media sources to point to your own league artwork.

### Step 3 - Add your teams

Each team gets a folder inside `Resources/Teams/`. The folder name is what
appears in the team dropdown.

Inside each folder:

- `Roster.txt` - one player name per line, in role order: Solo, Jungle, Mid, Support, Carry. Use `-` for an empty slot.
- `BanData.json` - tracks historical ban counts. The app updates this automatically when you submit ban data after a game.

Blank templates are already included. To add a new team, copy a blank folder,
rename it, and fill in the roster.

---

## Operator Usage Guide

### Before the match

1. Launch `SmitePnB.exe`
2. Select Team One and Team Two from the dropdowns
3. Switch OBS to the `PNBS` scene
4. You are ready

### During the draft

**Bans**

Each team has 5 ban slots. For each ban:

- pick the god from the dropdown
- check `Hover` to show the ban on stream before it is confirmed
- check `Lock` to commit the ban to the display and play the audio callout

**Picks**

Each team has 5 pick slots labeled by role. For each pick:

- pick the god from the dropdown
- check `Lock` to commit the pick to the display and play the audio callout

**Scores**

Update the score boxes at the top as games in the series are won.

**God names toggle**

The `God Names` checkbox in the top bar shows or hides the god name text on
top of the pick image.

### After the game

Click `Submit Bans` to record this game's ban data to the team files. This
updates the historical ban counts used in the top-bans display.

Then click `New Draft` to clear the board for the next game.

### Swapping sides

If teams need to swap left/right sides between games, click `Swap Teams` in
the top bar. The dropdowns flip and the display updates immediately.

### If something goes wrong

If the app crashes or needs a restart mid-draft, just relaunch it. You will be
asked if you want to restore the last saved state.

---

## Customizing the display

### Moving things around

Open **Settings -> Open layout.json in default editor**.

The file is plain JSON with named positions. Save the file and relaunch, or
hit Save in Settings to apply it.

If you make a mess of it, **Settings -> Reset layout to defaults** puts
everything back.

### Fonts and colors

In the Settings window:

- pick any font installed on your machine
- set hex color codes for god names, team names, and score text
- color swatches update live so you can see what you are getting

### Role labels

In Settings, under Role Labels, enter five entries: one per line.

---

## Adding new gods

When a new god ships in Smite 2:

1. Add the god name to `Resources/CharactersList.txt`
2. Drop their images into `Resources/CharacterImages/Picks/`, `Bans/`, and `TopBans/`
3. Drop their audio callout into `Resources/Sounds/`

No restart or reinstall is needed. The app reads the files fresh each launch.

---

## Project layout

```text
SmitePnB/
|-- SmitePnB.sln
|-- SmitePnB/
|   |-- Models/
|   |-- Services/
|   `-- Windows/
`-- Resources/
    |-- CharactersList.txt
    |-- CharacterImages/
    |-- Sounds/
    |-- Teams/
    `-- Display/
        `-- layout.json
```

### Building locally

From the repo root:

```bat
build.bat
```

This script:

- builds the app as a self-contained Windows executable
- copies `Resources/` beside the exe
- writes the runnable bundle to `build/SmitePnB/`

Launch:

```text
build\SmitePnB\SmitePnB.exe
```

If you want to publish manually instead of using the script:

```powershell
dotnet publish SmitePnB\SmitePnB\SmitePnB.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true --output build\publish
```

The publish output alone is not the full runnable bundle. Keep the copied
`Resources/` folder beside the exe.

---

## Testing

From the `SmitePnB/` folder that contains the solution file:

```powershell
dotnet test
```

The automated tests cover the core data layer: draft state, serialization,
resource loading, and team ban tracking. The WPF UI and audio playback still
need manual verification before live use.

When everything passes you should see output like:

```text
Passed!  - Failed:     0, Passed:    51, Skipped:     0, Total:    51
```

---

## Contributing

Pull requests are welcome. If you are adding gods, fixing assets, or improving
the display, fork the repo, make your changes, and open a PR.
