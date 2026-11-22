using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public class UIWorldGenTab : UIElement
{
    private readonly Asset<Texture2D> texture;
    private readonly Asset<Texture2D> hoverTexture;

    private readonly LocalizedText title;

    public bool Enabled;

    public UIWorldGenTab(LocalizedText title, Asset<Texture2D> texture)
    {
        Width.Set(44f, 0f);
        Height.Set(44f, 0f);

        this.texture = texture;
        hoverTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelBorder");

        this.title = title;
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var dims = this.Dimensions;
        Color color = Color.White;

        if (!Enabled)
        {
            color *= 0.4f;
            color.A = byte.MaxValue;
        }

        Utils.DrawSplicedPanel(sb, texture.Value, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, color);
        
        if (IsMouseHovering)
        {
            Utils.DrawSplicedPanel(sb, hoverTexture.Value, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, Color.White);

            Vector2 textPosition = new(dims.X + dims.Width + 4, dims.Y + 13);
            string text = title.Value;

            Utils.DrawBorderString(
                sb,
                text,
                textPosition,
                Color.White
            );
        }
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (Enabled)
        {
            return;
        }

        SoundEngine.PlaySound(SoundID.MenuOpen);
        base.LeftMouseDown(evt);
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }
}