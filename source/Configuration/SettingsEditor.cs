using System.Linq;
using Bubbles.Core;
using UnityEngine;
using Verse;

namespace Bubbles.Configuration
{
  public static class SettingsEditor
  {
    private static string[] _colorBuffer = new string[4];

    private static Vector2 _scrollPosition = Vector2.zero;
    private static Rect _viewRect;

    public static void DrawSettings(Rect rect)
    {
      var listingRect = new Rect(rect.x, rect.y + 40f, rect.width, rect.height - 40f);
      var l = new Listing_Settings();
      l.Begin(rect);

      if (l.ButtonText("Bubbles.ResetToDefault".Translate())) { Reset(); }

      l.End();
      l.BeginScrollView(listingRect, ref _scrollPosition, ref _viewRect);

      l.CheckboxLabeled("Bubbles.DoNonPlayer".Translate(), ref Settings.DoNonPlayer);
      l.CheckboxLabeled("Bubbles.DoAnimals".Translate(), ref Settings.DoAnimals);
      l.CheckboxLabeled("Bubbles.DoDrafted".Translate(), ref Settings.DoDrafted);
      var doTextColors = Settings.DoTextColors;
      l.CheckboxLabeled("Bubbles.DoTextColors".Translate(), ref Settings.DoTextColors);
      if (doTextColors != Settings.DoTextColors) { Bubbler.Rebuild(); }
      l.Gap();

      l.SliderLabeled("Bubbles.AutoHideSpeed".Translate(), ref Settings.AutoHideSpeed, 1, 5, display: Settings.AutoHideSpeed == 5 ? "Bubbles.AutoHideSpeedOff".Translate().ToString() : Settings.AutoHideSpeed.ToString());

      l.SliderLabeled("Bubbles.AltitudeBase".Translate(), ref Settings.AltitudeBase, 3, 44);
      l.SliderLabeled("Bubbles.AltitudeMax".Translate(), ref Settings.AltitudeMax, 20, 60);
      l.SliderLabeled("Bubbles.ScaleMax".Translate(), ref Settings.ScaleMax, 1f, 5f, 0.05f, Settings.ScaleMax.ToStringPercent());
      l.SliderLabeled("Bubbles.PawnMax".Translate(), ref Settings.PawnMax, 1, 15);

      l.SliderLabeled("Bubbles.FontSize".Translate(), ref Settings.FontSize, 5, 30);
      l.SliderLabeled("Bubbles.PaddingX".Translate(), ref Settings.PaddingX, 1, 40);
      l.SliderLabeled("Bubbles.PaddingY".Translate(), ref Settings.PaddingY, 1, 40);
      l.SliderLabeled("Bubbles.WidthMax".Translate(), ref Settings.WidthMax, 100, 500, 4);

      l.SliderLabeled("Bubbles.OffsetSpacing".Translate(), ref Settings.OffsetSpacing, 2, 12);
      l.SliderLabeled("Bubbles.OffsetStart".Translate(), ref Settings.OffsetStart, 0, 400, 2);

      var offsetDirection = Settings.OffsetDirection.AsInt;
      l.SliderLabeled("Bubbles.OffsetDirection".Translate(), ref offsetDirection, 0, 3, display: "Bubbles.OffsetDirections".Translate().ToString().Split('|').ElementAtOrDefault(offsetDirection));
      Settings.OffsetDirection = new Rot4(offsetDirection);

      l.SliderLabeled("Bubbles.OpacityStart".Translate(), ref Settings.OpacityStart, 0.3f, 1f, 0.05f, Settings.OpacityStart.ToStringPercent());
      l.SliderLabeled("Bubbles.OpacityHover".Translate(), ref Settings.OpacityHover, 0.05f, 1f, 0.05f, Settings.OpacityHover.ToStringPercent());
      l.SliderLabeled("Bubbles.FadeStart".Translate(), ref Settings.FadeStart, 100, 5000, 50);
      l.SliderLabeled("Bubbles.FadeLength".Translate(), ref Settings.FadeLength, 50, 2500, 50);

      l.ColorEntry("Bubbles.Background".Translate(), ref _colorBuffer[0], ref Settings.Background);
      l.ColorEntry("Bubbles.Foreground".Translate(), ref _colorBuffer[1], ref Settings.Foreground);
      l.ColorEntry("Bubbles.BackgroundSelected".Translate(), ref _colorBuffer[2], ref Settings.SelectedBackground);
      l.ColorEntry("Bubbles.ForegroundSelected".Translate(), ref _colorBuffer[3], ref Settings.SelectedForeground);

      l.EndScrollView(ref _viewRect);
    }

    private static void Reset()
    {
      _colorBuffer = new string[4];

      Settings.Reset();
    }

    public static void ShowWindow() => Find.WindowStack.Add(new Dialog());

    private class Dialog : Window
    {
      public override Vector2 InitialSize => new Vector2(600f, 600f);

      public Dialog()
      {
        optionalTitle = $"<b>{Mod.Name}</b>";
        doCloseX = true;
        doCloseButton = true;
        draggable = true;

        _viewRect = default;
      }

      public override void DoWindowContents(Rect rect)
      {
        rect.yMax -= 60f;
        DrawSettings(rect.ContractedBy(8f, 0f));
      }

      public override void PostClose()
      {
        Mod.Instance.WriteSettings();
        _viewRect = default;
      }
    }
  }
}
