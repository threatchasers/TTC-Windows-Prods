# BlackWallpaper

One-file on/off switch: **ON** = solid black desktop (the wallpaper picture is removed and the desktop color set to
black); **OFF** = the wallpaper and color that were there come back. Double-click toggles; arguments `on`, `off`,
`status`. No prompts, a small balloon says what happened. If the wallpaper was a slideshow, OFF restores the picture
that was showing when it was turned ON.

| File | What |
|---|---|
| `BlackWallpaper.cs` | the code (C#, .NET Framework 4, the compiler ships with Windows) |
| `build.ps1` | draws the icon (half-white, half-black monitor), compiles `BlackWallpaper.exe`, copies it to the Desktop |

State while ON: `%LOCALAPPDATA%\BlackWallpaper\saved.txt` (picture path and desktop color).
Tested 2026-11-11: ON removed the picture, OFF put the same picture back.

## License

MIT -- see the [LICENSE](../LICENSE) at the repository root.

Prebuilt `.exe` files are not in the repository: run `build.ps1` (no install needed).
