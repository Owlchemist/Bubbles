using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Bubbles.Core
{
  public static class Bubbler
  {
    private const float LabelPositionOffset = -0.6f;

    private static readonly Dictionary<Pawn, List<Bubble>> Dictionary = new Dictionary<Pawn, List<Bubble>>();

    public static void Add(LogEntry entry)
    {
      if (!GetApplicable() || !(entry is PlayLogEntry_Interaction interaction)) { return; }

      var initiator = interaction.initiator;
      var recipient = interaction.recipient;
      if (initiator == null || initiator.Map != Find.CurrentMap) { return; }

      if (!Settings.DoNonPlayer && (!initiator.Faction?.IsPlayer ?? true)) { return; }
      if (!Settings.DoAnimals && ((initiator?.RaceProps?.Animal ?? false) || (recipient?.RaceProps?.Animal ?? false))) { return; }
      if (!Settings.DoDrafted && ((initiator.drafter?.Drafted ?? false) || (recipient?.drafter?.Drafted ?? false))) { return; }

      if (!Dictionary.ContainsKey(initiator)) { Dictionary[initiator] = new List<Bubble>(); }

      Dictionary[initiator].Add(new Bubble(initiator, interaction));
    }

    private static void Remove(Pawn pawn, Bubble bubble)
    {
      Dictionary[pawn].Remove(bubble);
      if (Dictionary[pawn].Count == 0) { Dictionary.Remove(pawn); }
    }

    public static void Draw()
    {
      if (!GetApplicable()) { return; }

      var altitude = GetAltitude();
      if (altitude <= 0 || altitude > Settings.AltitudeMax) { return; }

      var scale = Settings.AltitudeBase / altitude;
      if (scale > Settings.ScaleMax) { scale = Settings.ScaleMax; }

      var selected = Find.Selector.SingleSelectedObject as Pawn;

      foreach (var pawn in Dictionary.Keys.OrderBy(pawn => pawn == selected).ThenBy(pawn => pawn.Position.y)) { DrawBubble(pawn, pawn == selected, scale); }
    }

    private static void DrawBubble(Pawn pawn, bool isSelected, float scale)
    {
      Map map = pawn.Map;
      if (!pawn.Spawned || map != Find.CurrentMap || map.fogGrid.IsFogged(pawn.Position)) { return; }

      var pos = GenMapUI.LabelDrawPosFor(pawn, LabelPositionOffset);

      var offset = Settings.OffsetStart;
      var count = 0;

      foreach (var bubble in Dictionary[pawn].OrderByDescending(b => b.Entry.Tick))
      {
        if (count > Settings.PawnMax) { return; }
        if (!bubble.Draw(pos + GetOffset(offset), isSelected, scale)) { Remove(pawn, bubble); }
        offset += (Settings.OffsetDirection.IsHorizontal ? bubble.Width : bubble.Height) + Settings.OffsetSpacing;
        count++;
      }
    }

    private static bool GetApplicable() => Settings.Activated && !WorldRendererUtility.WorldRenderedNow && (Settings.AutoHideSpeed == 5 || (int) Find.TickManager.CurTimeSpeed < Settings.AutoHideSpeed);

    private static float GetAltitude()
    {
      var altitude = System.Math.Max(1f, Current.cameraDriverInt.rootSize);
      Compatibility.Apply(ref altitude);

      return altitude;
    }

    private static Vector2 GetOffset(float offset)
    {
      var direction = Settings.OffsetDirection.AsVector2;
      return new Vector2(offset * direction.x, offset * direction.y);
    }

    public static void Rebuild() => Dictionary.Values.Do(list => list.Do(bubble => bubble.Rebuild()));

    public static void Clear() => Dictionary.Clear();
  }
}
