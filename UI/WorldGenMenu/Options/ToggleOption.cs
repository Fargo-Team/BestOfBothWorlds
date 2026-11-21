using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public sealed class ToggleOption(
    Mod mod,
    LocalizedText title,
    LocalizedText description,
    float priority,
    bool defaultValue,
    Color textColor,
    string? iconTexturePath)
    : WorldGenOption<bool>(mod, title, description, priority, defaultValue, textColor, iconTexturePath)
{
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        Value = !Value;

        base.LeftMouseDown(evt);
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);

        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var panelColor = Colors.InventoryDefaultColor * 0.75f;
        var panelTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/PanelGrayscale").Value;

        var selectedColor = Color.Lerp(Colors.InventoryDefaultColor, Color.White, 0.7f) * 0.75f;
        var selectedTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelHighlight").Value;

        var borderColor = Color.White;
        var borderTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelBorder").Value;

        DrawPanel(sb, panelTexture, panelColor);

        if (Value)
        {
            DrawPanel(sb, selectedTexture, selectedColor);
        }

        if (IsMouseHovering)
        {
            DrawPanel(sb, borderTexture, borderColor);
        }

        DrawIcon(sb);
    }
}
