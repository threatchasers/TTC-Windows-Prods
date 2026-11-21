# KeepAwake

One-file on/off switch for a laptop: **ON** = never sleep or hibernate (AC and battery); **OFF** = puts back
exactly the timeouts that were there before. Double-click toggles; arguments `on`, `off`, `status`. No prompts, a small
balloon says what happened. The lid action is not touched (that setting is not exposed on this laptop).

| File | What |
|---|---|
| `KeepAwake.cs` | the code (C#, .NET Framework 4, the compiler ships with Windows) |
| `build.ps1` | draws the coffee-cup icon, compiles `KeepAwake.exe`, copies it to the Desktop |

State while ON: `%LOCALAPPDATA%\KeepAwake\saved.txt` (the original timeouts). Delete it only if you want to forget them.
Tested 2026-11-11: ON then OFF returned the standby timeout to its original value.

## License

MIT -- see the [LICENSE](../LICENSE) at the repository root.

Prebuilt `.exe` files are not in the repository: run `build.ps1` (no install needed).
