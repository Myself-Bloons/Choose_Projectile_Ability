<h1 align="center">
<a href="https://github.com/Myself/Replace_Projectile_Ability/releases/latest/download/Replace_Projectile_Ability.dll">
    <img align="left" alt="Icon" height="90" src="con.png">
    <img align="right" alt="Download" height="75" src="https://raw.githubusercontent.com/gurrenm3/BTD-Mod-Helper/master/BloonsTD6%20Mod%20Helper/Resources/DownloadBtn.png">
</a>

</h1>
<h3 align="center">Choose a tower; your current tower now fires that tower’s projectiles and gains its ability.</h3>
<h1 align="center">Replace Projectile and Ability</h1>


[![Requires BTD6 Mod Helper](https://raw.githubusercontent.com/gurrenm3/BTD-Mod-Helper/master/banner.png)](https://github.com/gurrenm3/BTD-Mod-Helper#readme)

## Features

- Replace tower projectiles with any ability's projectiles
- Choose ability source via in-game popup (F8)
- Toggle replacement on/off with F9 hotkey
- Toggle via in-game mod settings menu
- Case-sensitive tower path selection (e.g., "TackShooter-050" for Super Maelstrom)
- Works with straight-traveling projectiles and boomerangs
- Ability logs all available tower paths and abilities on game load

## Installation

1. Install [BTD6 Mod Helper](https://github.com/gurrenm3/BTD-Mod-Helper#readme)
2. Download the latest release from the [Releases page](https://github.com/Myself/Replace_Projectile_Ability/releases)
3. Place `Replace_Projectile_Ability.dll` in your BTD6 Mods folder

## Usage

- The mod is **disabled by default** when you start the game
- Press **F8** to open the ability source selection popup
  - Enter an exact tower path (case-sensitive), like `TackShooter-050`
  - The popup will validate and show an error if the tower doesn't exist or has no ability
- Press **F9** at any time to toggle the replacement on/off
- Or use the Mod Helper settings menu in-game
- Check your MelonLoader console for a list of all available tower paths and their abilities

## How It Works

When enabled, the mod:
1. Takes the ability from your chosen source tower (e.g., Super Maelstrom from TackShooter-050)
2. Replaces the ability's projectiles with each tower's own projectiles
3. Adds this modified ability to newly placed or upgraded towers
4. Only affects towers with straight-traveling projectiles or boomerangs

## Compatibility

- Works on BTD6 version **51.1**
- Requires BTD6 Mod Helper
