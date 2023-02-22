using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace Bubbles.Core
{
  public class Bubble
  {
    private static readonly Regex RemoveColorTag = new Regex("<\\/?color[^>]*>");
    private static readonly GUIContent Content = new GUIContent();

    public PlayLogEntry_Interaction Entry { get; }

    private readonly Pawn _pawn;

    private string _text;
    public string Text => _text ?? (_text = GetText());

    private GUIStyle _style;
    private GUIStyle Style => _style ?? (_style = new GUIStyle(Verse.Text.CurFontStyle)
    {
      alignment = TextAnchor.MiddleCenter,
      clipping = TextClipping.Clip
    });

    public int Height { get; private set; }
    public int Width { get; private set; }

    public Bubble(Pawn pawn, PlayLogEntry_Interaction entry)
    {
      Entry = entry;
      _pawn = pawn;
    }

    public bool Draw(Vector2 pos, bool isSelected, float scale)
    {
      ScaleFont(ref scale);
      ScaleDimensions(scale);
      ScalePadding(scale);

      var posX = pos.x;
      var posY = pos.y;

      if (Settings.OffsetDirection.IsHorizontal) { posY -= Height / 2f; }
      else { posX -= Width / 2f; }

      var rect = new Rect((float)System.Math.Ceiling(posX), (float)System.Math.Ceiling(posY), Width, Height).RoundedCeil();

      var fade = Event.current.shift && Mouse.IsOver(rect) ? Settings.OpacityStart : System.Math.Min(GetFade(), Mouse.IsOver(rect) ? Settings.OpacityHover : 1f);
      if (fade <= 0f) { return false; }

      var background = GetBackground(isSelected).ToTransparent(fade);
      var foreground = GetForeground(isSelected).ToTransparent(fade);

      var prevColor = GUI.color;

      GUI.color = background;
      DrawAtlas(rect, Textures.Inner);

      GUI.color = foreground;
      DrawAtlas(rect, Textures.Outer);
      GUI.Label(rect, Text, Style);

      GUI.color = prevColor;

      return true;
    }

    private void ScaleFont(ref float scale)
    {
      Style.fontSize = (int)System.Math.Round(Settings.FontSize * scale);
      scale = Style.fontSize / (float) Settings.FontSize;
    }

    private void ScalePadding(float scale)
    {
      var paddingX = (int)System.Math.Round(Settings.PaddingX * scale);
      var paddingY = (int)System.Math.Round(Settings.PaddingY * scale);
      Style.padding = new RectOffset(paddingX, paddingX, paddingY, paddingY);
    }

    private void ScaleDimensions(float scale)
    {
      Content.text = Text;
      Width = (int)System.Math.Round(System.Math.Min(Style.CalcSize(Content).x, Settings.WidthMax * scale));
      Height = (int)System.Math.Round(Style.CalcHeight(Content, Width));
    }

    private string GetText()
    {
      var text = Entry.ToGameStringFromPOV(_pawn);
      return Settings.DoTextColors ? text : RemoveColorTag.Replace(text, "");
    }

    private float GetFade()
    {
      var elasped = Find.TickManager.TicksAbs - Entry.Tick - Settings.FadeStart;

      if (elasped <= 0) { return Settings.OpacityStart; }
      if (elasped > Settings.FadeLength) { return 0f; }

      var fade = Settings.OpacityStart * (1f - (elasped / (float) Settings.FadeLength));
      return fade;
    }

    private static Color GetBackground(bool isSelected) => isSelected ? Settings.SelectedBackground : Settings.Background;
    private static Color GetForeground(bool isSelected) => isSelected ? Settings.SelectedForeground : Settings.Foreground;

    static readonly Rect rect2A = new Rect(0.0f, 0.0f, 0.25f, 0.25f),
      rect4A = new Rect(0.75f, 0.0f, 0.25f, 0.25f),
      rect6A = new Rect(0.0f, 0.75f, 0.25f, 0.25f),

      rect2B = new Rect(0.75f, 0.75f, 0.25f, 0.25f),
      rect4B = new Rect(0.25f, 0.25f, 0.5f, 0.5f),
      rect6B = new Rect(0.25f, 0.0f, 0.5f, 0.25f),

      rect2C = new Rect(0.25f, 0.75f, 0.5f, 0.25f),
      rect4C = new Rect(0.0f, 0.25f, 0.25f, 0.5f),
      rect6C = new Rect(0.75f, 0.25f, 0.25f, 0.5f);
    
    private static void DrawAtlas(Rect rect, Texture2D atlas)
    {
      rect.xMin = Widgets.AdjustCoordToUIScalingFloor(rect.xMin);
      rect.yMin = Widgets.AdjustCoordToUIScalingFloor(rect.yMin);
      rect.xMax = Widgets.AdjustCoordToUIScalingCeil(rect.xMax);
      rect.yMax = Widgets.AdjustCoordToUIScalingCeil(rect.yMax);

      var scale = (int)System.Math.Round(Mathf.Min(atlas.width * 0.25f, rect.height / 4f, rect.width / 4f));

      GUI.BeginGroup(rect);

      Widgets.DrawTexturePart(new Rect(0.0f, 0.0f, scale, scale), rect2A, atlas);
      Widgets.DrawTexturePart(new Rect(rect.width - scale, 0.0f, scale, scale), rect4A, atlas);
      Widgets.DrawTexturePart(new Rect(0.0f, rect.height - scale, scale, scale), rect6A, atlas);

      Widgets.DrawTexturePart(new Rect(rect.width - scale, rect.height - scale, scale, scale), rect2B, atlas);
      Widgets.DrawTexturePart(new Rect(scale, scale, rect.width - (scale * 2f), rect.height - (scale * 2f)), rect4B, atlas);
      Widgets.DrawTexturePart(new Rect(scale, 0.0f, rect.width - (scale * 2f), scale), rect6B, atlas);

      Widgets.DrawTexturePart(new Rect(scale, rect.height - scale, rect.width - (scale * 2f), scale), rect2C, atlas);
      Widgets.DrawTexturePart(new Rect(0.0f, scale, scale, rect.height - (scale * 2f)), rect4C, atlas);
      Widgets.DrawTexturePart(new Rect(rect.width - scale, scale, scale, rect.height - (scale * 2f)), rect6C, atlas);

      GUI.EndGroup();
    }

    public void Rebuild() => _text = null;
  }
}
