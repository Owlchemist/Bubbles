using Bubbles.Configuration;
using Bubbles.Core;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Bubbles.Patch
{
  [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
  public static class RimWorld_PlaySettings_DoPlaySettingsGlobalControls
  {
    static readonly string translatedString = "Bubbles.Toggle".Translate();
    private static void Postfix(WidgetRow row, bool worldView)
    {
      if (worldView || row == null) { return; }

      var activated = Settings.Activated;
      row.ToggleableIcon(ref activated, Textures.Icon, translatedString, SoundDefOf.Mouseover_ButtonToggle);

      if (activated != Settings.Activated && Event.current.shift) { SettingsEditor.ShowWindow(); }
      else { Settings.Activated = activated; }
    }
  }
}
