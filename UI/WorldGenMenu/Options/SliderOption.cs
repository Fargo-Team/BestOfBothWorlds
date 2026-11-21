using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargoSeeds.UI.WorldGenMenu;

public sealed class SliderOption(
    Mod mod,
    LocalizedText title,
    LocalizedText description,
    float priority,
    float defaultValue,
    int segments,
    float min,
    float max,
    Color textColor,
    string? iconTexturePath)
    : WorldGenOption<float>(mod, title, description, priority, defaultValue, textColor, iconTexturePath)
{
    public override void OnInitialize()
    {
        var slider = new UISlider();
        {
            slider.Segments = segments;
            slider.HAlign = 1f;
            slider.Width.Set(-30, 1f);
            slider.VAlign = 0.5f;

            slider.OnChanged += OnChanged_UpdateValue;
        }
        Append(slider);
    }

    private void OnChanged_UpdateValue(UISlider obj)
    {
        Value = Utils.Remap(obj.Ratio, 0f, 1f, min, max);
    }
}
