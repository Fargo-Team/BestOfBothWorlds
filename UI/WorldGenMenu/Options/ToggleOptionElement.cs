using FargoSeeds.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public sealed class ToggleOptionElement(
    WorldGenOption option,
    Color textColor)
    : UIOptionElement<bool>(option, textColor)
{
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        Value = !Value;

        base.LeftMouseDown(evt);
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
            var dims = this.Dimensions;
            dims.Inflate(-7, -7);

            DrawPanel(sb, selectedTexture, selectedColor, dims);
        }

        if (IsMouseHovering)
        {
            DrawPanel(sb, borderTexture, borderColor);
        }

        DrawIcon(sb);
    }

    private bool priorHover;

    public override void Update(GameTime gameTime)
    {
        if (!priorHover && IsMouseHovering)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        priorHover = IsMouseHovering;

        base.Update(gameTime);
    }
}
