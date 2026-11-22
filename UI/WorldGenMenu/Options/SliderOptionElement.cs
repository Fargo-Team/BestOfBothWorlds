using FargoSeeds.Content;
using Microsoft.Xna.Framework;
using Terraria;

namespace FargoSeeds.UI.WorldGenMenu;

public sealed class SliderOptionElement(
    WorldGenOption option,
    float min,
    float max,
    int segments,
    Color textColor)
    : UIOptionElement<float>(option, textColor)
{
    public override void OnInitialize()
    {
        var slider = new UISlider();
        {
            slider.Segments = segments;
            slider.HAlign = 1f;
            slider.Width.Set(-40, 1f);
            slider.VAlign = 0.5f;

            slider.Left.Set(-5, 0f);

            slider.BorderColor = Color.Gray;
            slider.InnerColor = Color.DarkGray;

            slider.OnChanged += OnChanged_UpdateValue;

            slider.Ratio = Utils.Remap(Value, min, max, 0f, 1f);
        }
        Append(slider);

        RemoveChild(Label);
        Append(Label);

        Label.SetText($"{Title}: {Value}");

        Recalculate();
    }

    private void OnChanged_UpdateValue(UISlider obj)
    {
        Value = Utils.Remap(obj.Ratio, 0f, 1f, min, max);

        Label.SetText($"{Title}: {Value}");
    }
}
