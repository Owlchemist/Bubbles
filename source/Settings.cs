using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bubbles.Configuration;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Bubbles
{
  public class Settings : ModSettings
  {
    private static readonly string[] SameConfigVersions =
    {
      "2.4"
    };
    private static bool _resetRequired;

    public static bool Activated = true;

    public static int AutoHideSpeed = 5;

    public static bool DoNonPlayer = true;
    public static bool DoAnimals = true;
    public static bool DoDrafted;
    public static bool DoTextColors;

    public static int AltitudeBase = 11;
    public static int AltitudeMax = 40;
    public static float ScaleMax = 1.25f;
    public static int PawnMax = 3;

    public static int FontSize = 12;
    public static int PaddingX = 7;
    public static int PaddingY = 5;
    public static int WidthMax = 256;

    public static int OffsetSpacing = 2;
    public static int OffsetStart = 14;
    public static Rot4 OffsetDirection = Rot4.North;

    public static float OpacityStart = 0.9f;
    public static float OpacityHover = 0.2f;

    public static int FadeStart = 500;
    public static int FadeLength = 100;

    public static Color Background = Color.white;
    public static Color Foreground = Color.black;
    public static Color SelectedBackground = new Color(1f, 1f, 0.75f);
    public static Color SelectedForeground = Color.black;

    private static IEnumerable<Setting> AllSettings => typeof(Settings).GetFields().Select(field => field.GetValue(null) as Setting).Where(setting => setting != null);

    public static void Reset() => AllSettings.Do(setting => setting.ToDefault());

    public void CheckResetRequired()
    {
      if (!_resetRequired) { return; }
      _resetRequired = false;

      Write();

      Bubbles.Mod.Warning("Settings were reset with new update");
    }

    public override void ExposeData()
    {
      if (_resetRequired) { return; }

      var version = Scribe.mode == LoadSaveMode.Saving ? Bubbles.Mod.Version : null;
      Scribe_Values.Look(ref version, "Version");
      if (Scribe.mode == LoadSaveMode.LoadingVars && (version == null || (version != Bubbles.Mod.Version && !SameConfigVersions.Contains(Regex.Match(version, "^\\d+\\.\\d+").Value))))
      {
        _resetRequired = true;
        return;
      }

      AllSettings.Do(setting => setting.Scribe());
    }
  }
}
