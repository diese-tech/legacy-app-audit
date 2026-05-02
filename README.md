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

## What you need

- Windows 10 or 11
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (free, one-time install)
- OBS Studio

---

## Setup Guide

### Step 1 — Install the .NET runtime

Download and run the **.NET 8 Desktop Runtime** from Microsoft's site (link above).
You only need to do this once per machine.

### Step 2 — Set your resources path

On first launch, SmitePnB will ask you to point it at the Resources folder.
That folder contains everything the app needs: god images, sounds, and team data.

If you're running from this repo, that path will be something like:
```
C:\Projects\legacy-app-audit\SmitePnB\Resources
```

Set it in the Settings window (⚙ button in the top bar) and hit Save.

### Step 3 — Import the OBS scene collection

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

### Step 4 — Add your teams

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

---

## Contributing

Pull requests welcome. If you're adding gods, fixing assets, or improving the
display — just fork, make your changes, and open a PR. The goal is for this
to be community-maintained so it doesn't disappear when one person moves on.
