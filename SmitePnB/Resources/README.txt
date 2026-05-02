SmitePnB — Resources folder
============================

This folder is the only thing you need to update between patches or seasons.
No recompile, no code changes needed.

CharactersList.txt
  One god name per line. Add new gods here when they ship.
  Names must exactly match the image and sound filenames below.

CharacterImages/
  Picks/     — <GodName>.png shown in pick slots
  Bans/      — <GodName>.png shown in ban slots (usually darker/greyed)
  TopBans/   — <GodName>.png shown in the historical top-bans panel

Sounds/
  <GodName>.mp3  — audio callout played on lock-in
  hover.wav      — played when a god is selected in a slot
  lockin.wav     — played when the lock checkbox is checked

Teams/
  One folder per team. Folder name is what appears in the team dropdown.
  Each folder contains:
    Roster.txt     — 5 player names, one per line, in role order:
                     Solo / Jungle / Mid / Support / Carry
                     Use - for an empty slot (e.g. Joust format)
    BanData.json   — historical ban counts. Auto-updated on Submit Bans.
                     teamname: display name shown on stream
                     totalgames: total games tracked
                     bancounts: { "GodName": count, ... }

Display/
  layout.json  — pixel positions for every element on the display window.
                 Edit this file to move things around. Open it from
                 Settings → "Open layout.json in default editor".
                 Reset to defaults from Settings → "Reset layout to defaults".
