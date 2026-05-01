# SAL PROD — Smite 2 Pick-and-Ban Broadcasting Suite

A Windows desktop application and OBS configuration package for running
live pick-and-ban overlays during competitive Smite 2 tournament broadcasts.

## Overview

SAL PROD provides tournament operators with a real-time draft tracking
interface integrated directly into OBS. It manages character picks/bans,
team rosters, historical ban data, and audio callouts for on-stream display.

## Requirements

- Windows 10/11
- .NET Framework 4.7.2
- OBS Studio

## Project Structure

```
sal-prod/
├── README.md
├── SAL_PROD.json               # OBS scene collection (import this into OBS)
├── Assets/                     # Branding and screenshot assets
└── Smite 2 by Tes/
    ├── 5 Bans/                 # Main application (tournament pick/ban mode)
    │   ├── Application/        # Executable and .NET dependencies
    │   │   ├── Smite_PnB_Layout.exe
    │   │   ├── Data.txt        # Path and display configuration
    │   │   └── default.layout  # UI element positioning
    │   └── Resources/
    │       ├── CharactersList.txt      # 80+ Smite 2 gods
    │       ├── CharacterImages/        # Pick, Ban, and TopBan image sets
    │       ├── Sounds/                 # Per-god audio callouts + SFX
    │       ├── Teams/                  # 9 team configurations
    │       │   ├── Roster.txt          # Per-team role assignments
    │       │   └── BanData.json        # Per-team historical ban stats
    │       └── Display/                # Layout templates and backups
    └── Vault Bans/             # HTML reference tool for character catalog
        └── god_image.html
```

## Setup

### 1. Configure the resource path

Open `Smite 2 by Tes/5 Bans/Application/Data.txt` and set the path to
match your local installation:

```
-resources_path-: C:/Projects/sal-prod
-resolution-: 0
-show_god_names-: True
```

### 2. Import the OBS scene collection

In OBS Studio: **Scene Collection → Import → select `SAL_PROD.json`**

The collection includes these broadcast scenes:

| Scene | Purpose |
|-------|---------|
| STARTING SOON | Pre-show countdown |
| BRB | Be Right Back screen |
| DESK | Analyst desk / commentary |
| PNBS | Pick-and-ban overlay (live draft) |
| GAME | In-game capture |
| POST GAME | End-of-match results |

### 3. Configure teams

Each team lives in `Resources/Teams/<team name>/`:
- **`Roster.txt`** — player names mapped to roles (Solo, Jungle, Mid, Support, Carry)
- **`BanData.json`** — character ban history for stat tracking

Blank templates are included for new teams. Joust-mode variants are
also available for 3v3 formats.

## Usage

1. Launch `Smite_PnB_Layout.exe`
2. Select the two competing teams
3. As the in-game draft proceeds, click characters to register picks/bans
4. The overlay updates in real time and plays audio callouts
5. Switch OBS scenes as the broadcast progresses

## Character Reference

Open `Smite 2 by Tes/Vault Bans/god_image.html` in a browser to browse
the full character pool with images.

## Assets

- **80+ gods** with pick image, ban image, and top-ban image variants
- **160+ MP3 audio callouts** (one per character)
- Sound effects: `hover.wav`, `lockin.wav`
