using FargoSeeds.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public class UIOptionElement : UIElement
{
    public readonly WorldGenOption Option;

    public readonly LocalizedText Title;

    public readonly LocalizedText Description;

    protected UIText Label;

    public UIOptionElement(
        WorldGenOption option,
        Color textColor
    )
    {
        Option = option;

        Title = Language.GetText(Option.Name);
        Description = Language.GetText(Option.Description);

        Label = new(Title, 0.9f);
        {
            Label.HAlign = 1f;
            Label.VAlign = 0.5f;
            Label.Width.Set(0f - (Option.Icon is null ? 0f : 35f), 1f);
            Label.Top.Set(0f, 0f);
            Label.TextColor = textColor;
            Label.IgnoresMouseInteraction = true;
        }
        Append(Label);
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var panelTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/PanelGrayscale").Value;

        var panelColor = Colors.InventoryDefaultColor * 0.75f;

        DrawPanel(sb, panelTexture, panelColor);
        DrawIcon(sb);
    }

    protected void DrawPanel(SpriteBatch sb, Texture2D texture, Color color)
    {
        DrawPanel(sb, texture, color, this.Dimensions);
    }

    protected void DrawPanel(SpriteBatch sb, Texture2D texture, Color color, Rectangle dims)
    {
        Utils.DrawSplicedPanel(sb, texture, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, color);
    }

    protected void DrawIcon(SpriteBatch sb)
    {
        if (Option.Icon is null)
        {
            return;
        }

        var dims = this.Dimensions;

        var sizeRef = ModContent.Request<Texture2D>("Terraria/Images/UI/WorldCreation/IconEvilCorruption").Value;

        Color color2 = Color.White;
        Vector2 offset = (sizeRef.Size() - Option.Icon.Value.Size()) / 2;

        sb.Draw(Option.Icon.Value, offset + new Vector2(dims.X + 1f, dims.Y + 1f), color2);
    }

    public override int CompareTo(object? obj)
    {
        if (obj is not UIOptionElement opt)
        {
            return 0;
        }

        return Option.Priority.CompareTo(opt.Option.Priority);
    }
}

public class UIOptionElement<T>(
    WorldGenOption option,
    Color textColor)
    : UIOptionElement(option, textColor)
{
    protected T Value
    {
        get => (T)Option.Value!;
        set => Option.Value = value;
    }
}
