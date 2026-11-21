using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;

namespace FargoSeeds.UI;

public class UISlider : UIElement
{
    public Asset<Texture2D> InnerTexture { get; set; }

    public Asset<Texture2D> BlipTexture { get; set; }

    public Color InnerColor { get; set; }

    public float Ratio { get; set; }

    public int Segments { get; set; } = -1;

    public event Action<UISlider>? OnChanged;

    /// <summary>
    /// Used over IsMouseHovering to determine if the mouse is hovering the slider and NOT dragging another.
    /// </summary>
    public bool Hovering { get; private set; }

    protected bool IsHeld;

    public UISlider()
    {
        Width.Set(0, 1f);
        Height.Set(16, 0f);

        InnerColor = Color.White;

        InnerTexture = Assets.Images.UI.Gradient.Asset;
        BlipTexture = TextureAssets.ColorSlider;
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        base.LeftMouseDown(evt);

        if (evt.Target == this)
        {
            IsHeld = true;
        }
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        base.LeftMouseUp(evt);

        IsHeld = false;
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);

        Hovering = IsMouseHovering && (!Main.mouseLeft || IsHeld);

        if (!Hovering || IsHeld)
        {
            return;
        }

        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    public override void MouseOut(UIMouseEvent evt)
    {
        base.MouseOut(evt);

        Hovering = false;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Rectangle dims = this.Dimensions;

        var mousePosition = UserInterface.ActiveInstance.MousePosition;

        if (!IsHeld)
        {
            if (!Hovering && IsMouseHovering && !Main.mouseLeft)
            {
                Hovering = true;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }

            return;
        }

        float oldRatio = Ratio;

        float num = mousePosition.X - dims.X;

        Ratio = Math.Clamp(num / dims.Width, 0f, 1f);

        if (Segments != -1)
        {
            Ratio = float.Round(Ratio * Segments) / Segments;
        }

        if (oldRatio != Ratio)
        {
            OnChanged?.Invoke(this);
        }
    }

    protected virtual void DrawSliderInner(SpriteBatch sb, Rectangle dims, Color color)
    {
        sb.Draw(InnerTexture.Value, dims, color);
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);

        Rectangle dims = this.Dimensions;

        Texture2D slider = Assets.Images.UI.Slider.Asset.Value;
        Texture2D sliderOutline = Assets.Images.UI.SliderHighlight.Asset.Value;

        DrawBar(slider, Color.White);

        if (Hovering || IsHeld)
        {
            DrawBar(sliderOutline, Main.OurFavoriteColor);
        }

        dims.Inflate(-4, -4);

        DrawSliderInner(sb, dims, InnerColor);

        Texture2D blip = BlipTexture.Value;

        Vector2 blipOrigin = blip.Size() * 0.5f;
        Vector2 blipPosition = new Vector2(dims.X + Ratio * dims.Width, dims.Center.Y);

        sb.Draw(blip, blipPosition, null, Color.White, 0f, blipOrigin, 1f, 0, 0f);

        return;

        void DrawBar(Texture2D texture, Color color)
        {
            var startDest = new Rectangle(
                dims.X,
                dims.Y,
                6,
                dims.Height
            );

            var centerDest = new Rectangle(
                dims.X + 6,
                dims.Y,
                dims.Width - 12,
                dims.Height
            );

            var endDest = new Rectangle(
                dims.X + dims.Width - 6,
                dims.Y,
                6,
                dims.Height
            );

            sb.Draw(texture, startDest, new(0, 0, 6, texture.Height), color);
            sb.Draw(texture, centerDest, new(6, 0, 2, texture.Height), color);
            sb.Draw(texture, endDest, new(8, 0, 6, texture.Height), color);
        }
    }
}
