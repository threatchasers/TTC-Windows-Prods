// KeepAwake.cs -- one-file on/off switch that keeps this PC awake.
// Build (no install needed, .NET Framework 4 compiler that ships with Windows):
//   csc /nologo /target:winexe /win32icon:keepawake.ico /out:KeepAwake.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll KeepAwake.cs
// Use: double-click toggles. Arguments: on | off | status. No prompts; a small balloon says what happened.
// ON  = saves the current standby/hibernate timeouts, then sets them to never (AC and battery).
// OFF = puts back exactly what was saved. Lid action is not touched (not exposed on this PC).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

static class KeepAwake
{
    static readonly string[][] Items = new string[][] {
        new string[] { "SUB_SLEEP", "STANDBYIDLE" },
        new string[] { "SUB_SLEEP", "HIBERNATEIDLE" }
    };

    static string Powercfg(string args)
    {
        var psi = new ProcessStartInfo("powercfg.exe", args);
        psi.UseShellExecute = false; psi.CreateNoWindow = true; psi.RedirectStandardOutput = true;
        using (var p = Process.Start(psi)) { string o = p.StandardOutput.ReadToEnd(); p.WaitForExit(); return o; }
    }

    // Returns {ac, dc} in seconds, or null when the setting is not exposed.
    static long[] Read(string sub, string set)
    {
        var m = Regex.Matches(Powercfg("/q SCHEME_CURRENT " + sub + " " + set), "0x[0-9a-fA-F]{8}");
        if (m.Count < 2) return null;
        return new long[] { Convert.ToInt64(m[m.Count - 2].Value, 16), Convert.ToInt64(m[m.Count - 1].Value, 16) };
    }

    static void Write(string sub, string set, long ac, long dc)
    {
        Powercfg("/setacvalueindex SCHEME_CURRENT " + sub + " " + set + " " + ac);
        Powercfg("/setdcvalueindex SCHEME_CURRENT " + sub + " " + set + " " + dc);
    }

    static void Say(string text)
    {
        var ni = new NotifyIcon();
        try { ni.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { ni.Icon = SystemIcons.Information; }
        ni.Visible = true;
        ni.ShowBalloonTip(3000, "KeepAwake", text, ToolTipIcon.Info);
        Thread.Sleep(3500);
        ni.Visible = false; ni.Dispose();
    }

    [STAThread]
    static void Main(string[] args)
    {
        string dir = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "KeepAwake");
        string state = Path.Combine(dir, "saved.txt");
        bool isOn = File.Exists(state);
        string arg = args.Length > 0 ? args[0].ToLowerInvariant() : "toggle";
        if (arg == "toggle") arg = isOn ? "off" : "on";

        if (arg == "on")
        {
            if (isOn) { Say("Already ON (your original settings are saved)."); return; }
            Directory.CreateDirectory(dir);
            var lines = new List<string>();
            foreach (var it in Items)
            {
                var v = Read(it[0], it[1]);
                if (v != null) lines.Add(it[0] + "|" + it[1] + "|" + v[0] + "|" + v[1]);
            }
            File.WriteAllLines(state, lines.ToArray());
            foreach (var l in lines) { var f = l.Split('|'); Write(f[0], f[1], 0, 0); }
            Powercfg("/setactive SCHEME_CURRENT");
            Say("ON: this PC will not sleep or hibernate. Run again to turn it off.");
        }
        else if (arg == "off")
        {
            if (!isOn) { Say("Already OFF (nothing to restore)."); return; }
            foreach (var l in File.ReadAllLines(state))
            {
                var f = l.Split('|');
                if (f.Length == 4) Write(f[0], f[1], long.Parse(f[2]), long.Parse(f[3]));
            }
            Powercfg("/setactive SCHEME_CURRENT");
            File.Delete(state);
            Say("OFF: power settings are back as they were.");
        }
        else
        {
            var sb = new List<string>();
            foreach (var it in Items) { var v = Read(it[0], it[1]); sb.Add(it[1] + (v == null ? " n/a" : " AC=" + v[0] + " DC=" + v[1])); }
            Say((isOn ? "ON. " : "OFF. ") + string.Join("; ", sb.ToArray()));
        }
    }
}
