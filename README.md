# TT-Windows-Prods

Small, single-file Windows utilities. Each one is a C# source file plus a `build.ps1` that compiles it with the
C# compiler that ships with Windows (.NET Framework 4) -- nothing to install, no installer, no background service.

| Tool | What it does |
|---|---|
| [`BlackWallpaper`](BlackWallpaper/) | On/off switch: solid black desktop, then back to exactly the wallpaper and color you had. |
| [`KeepAwake`](KeepAwake/) | On/off switch: the PC never sleeps or hibernates (AC and battery), then back to the exact timeouts you had. |

Both work the same way: double-click toggles, or pass `on`, `off` or `status`; a small balloon says what happened;
the previous state is saved under `%LOCALAPPDATA%\<Tool>\saved.txt` so OFF restores it.

## Build

```powershell
cd KeepAwake        # or BlackWallpaper
.\build.ps1         # draws the icon, compiles the .exe, copies it to your Desktop
```

## Contact

Questions, ideas or collaboration: **[LinkedIn](https://www.linkedin.com/in/yrbusiness)**.
Bugs and improvements: GitHub issues and pull requests are welcome.

## License

MIT -- see [LICENSE](LICENSE).
