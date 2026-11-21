// BlackWallpaper.cs -- one-file on/off switch: solid black desktop, then back to the wallpaper you had.
// Build with build.ps1 (uses the C# compiler that ships with Windows).
// Double-click toggles. Arguments: on | off | status. No prompts; a small balloon says what happened.
// ON  = remembers the current wallpaper picture and desktop color, removes the picture, sets the color to black.
// OFF = puts back the picture and color it remembered.
// Note: if the wallpaper was a slideshow, OFF restores the picture that was showing at that moment.
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

static class BlackWallpaper
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int SystemParametersInfo(int action, int param, StringBuilder buffer, int flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern int SystemParametersInfo(int action, int param, string value, int flags);
    [DllImport("user32.dll")]
    static extern bool SetSysColors(int count, int[] elements, int[] colors);

    const int SPI_GETDESKWALLPAPER = 0x0073, SPI_SETDESKWALLPAPER = 0x0014, UPDATE_AND_BROADCAST = 0x03, COLOR_DESKTOP = 1;
    const string ColorsKey = @"Control Panel\Colors";

    static string CurrentWallpaper()
    {
        var sb = new StringBuilder(520);
        SystemParametersInfo(SPI_GETDESKWALLPAPER, sb.Capacity, sb, 0);
        return sb.ToString();
    }

    static string CurrentColor()
    {
        using (var k = Registry.CurrentUser.OpenSubKey(ColorsKey))
            return (k == null ? null : k.GetValue("Background") as string) ?? "0 0 0";
    }

    static void ApplyColor(string rgb)
    {
        var p = rgb.Split(' ');
        int r = int.Parse(p[0]), g = int.Parse(p[1]), b = int.Parse(p[2]);
        using (var k = Registry.CurrentUser.OpenSubKey(ColorsKey, true)) k.SetValue("Background", rgb);
        SetSysColors(1, new int[] { COLOR_DESKTOP }, new int[] { r | (g << 8) | (b << 16) });
    }

    static void Say(string text)
    {
        var ni = new NotifyIcon();
        try { ni.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { ni.Icon = SystemIcons.Information; }
        ni.Visible = true;
        ni.ShowBalloonTip(3000, "BlackWallpaper", text, ToolTipIcon.Info);
        Thread.Sleep(3500);
        ni.Visible = false; ni.Dispose();
    }

    [STAThread]
    static void Main(string[] args)
    {
        string dir = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "BlackWallpaper");
        string state = Path.Combine(dir, "saved.txt");
        bool isOn = File.Exists(state);
        string arg = args.Length > 0 ? args[0].ToLowerInvariant() : "toggle";
        if (arg == "toggle") arg = isOn ? "off" : "on";

        if (arg == "on")
        {
            if (isOn) { Say("Already ON (your wallpaper is saved)."); return; }
            Directory.CreateDirectory(dir);
            File.WriteAllLines(state, new string[] { CurrentWallpaper(), CurrentColor() });
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, "", UPDATE_AND_BROADCAST);
            ApplyColor("0 0 0");
            Say("ON: black desktop. Run again to bring your wallpaper back.");
        }
        else if (arg == "off")
        {
            if (!isOn) { Say("Already OFF (nothing to restore)."); return; }
            var lines = File.ReadAllLines(state);
            string pic = lines.Length > 0 ? lines[0] : "";
            string color = lines.Length > 1 ? lines[1] : "0 0 0";
            ApplyColor(color);
            if (pic.Length > 0 && File.Exists(pic)) SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, pic, UPDATE_AND_BROADCAST);
            File.Delete(state);
            Say("OFF: wallpaper and color are back as they were.");
        }
        else
        {
            Say((isOn ? "ON" : "OFF") + ". Wallpaper: " + (CurrentWallpaper().Length == 0 ? "(none, solid color)" : CurrentWallpaper()));
        }
    }
}
