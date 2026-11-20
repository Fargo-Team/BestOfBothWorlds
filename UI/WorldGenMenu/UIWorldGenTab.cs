using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public class UIWorldGenTab : UIElement
{

    private readonly Asset<Texture2D> _BasePanelTexture;

    private readonly Asset<Texture2D> _selectedBorderTexture;

    private readonly Asset<Texture2D> _hoveredBorderTexture;

    private bool _hovered;

    private bool _soundedHover;

    public readonly LocalizedText Title;

    private Func<bool> _shouldBeEnabled;

    private Action _onClick;

    public UIWorldGenTab(LocalizedText title, string panelTexturePath, Func<bool> shouldBeEnabled, Action onClick)
    {
        Width = StyleDimension.FromPixels(44f);
        Height = StyleDimension.FromPixels(44f);
        _BasePanelTexture = ModContent.Request<Texture2D>(panelTexturePath, (AssetRequestMode)1);
        _selectedBorderTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelHighlight", (AssetRequestMode)1);
        _hoveredBorderTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelBorder", (AssetRequestMode)1);
        Title = title;
        _shouldBeEnabled = shouldBeEnabled;
        _onClick = onClick;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (_hovered)
        {
            if (!_soundedHover)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            _soundedHover = true;
        }
        else
        {
            _soundedHover = false;
        }
        CalculatedStyle dimensions = GetDimensions();
        Color color = Color.White;
        float opacity = 1f;
        bool isSelected = _shouldBeEnabled.Invoke();

        float fade = isSelected ? 1f : 0.4f;


        Utils.DrawSplicedPanel(spriteBatch, _BasePanelTexture.Value, (int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height, 10, 10, 10, 10, Color.Lerp(Color.Black, color, fade) * opacity);
        if (_hovered)
        {
            Utils.DrawSplicedPanel(spriteBatch, _hoveredBorderTexture.Value, (int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height, 10, 10, 10, 10, Color.White);

            Vector2 textPosition = new(dimensions.X + dimensions.Width + 4, dimensions.Y + 10);
            string text = Title.Value;

            Utils.DrawBorderString(
                spriteBatch,
                text,
                textPosition,
                Color.White);
        }

        if (!WorldGenUIManager.HasClickedMenu && !isSelected) // blinking flashing baby sensory lights please look at my menu
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            float flashOpacity = (MathF.Sin(Main.GlobalTimeWrappedHourly * 12f) + 1) / 2;
            flashOpacity *= 0.65f;
            Utils.DrawSplicedPanel(spriteBatch, _BasePanelTexture.Value, (int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height, 10, 10, 10, 10, Color.White * opacity * flashOpacity);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (_shouldBeEnabled.Invoke())
            return;
        SoundEngine.PlaySound(SoundID.MenuTick);
        _onClick.Invoke();
        WorldGenUIManager.HasClickedMenu = true;
        base.LeftMouseDown(evt);
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        _hovered = true;
    }

    public override void MouseOut(UIMouseEvent evt)
    {
        base.MouseOut(evt);
        _hovered = false;
    }
}