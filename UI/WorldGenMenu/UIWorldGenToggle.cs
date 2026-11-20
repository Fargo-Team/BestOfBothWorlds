using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public class UIWorldGenToggle : UIElement
{
    private bool _enabled;

    public Mod Mod;

    private readonly Asset<Texture2D> _BasePanelTexture;

    private readonly Asset<Texture2D> _selectedBorderTexture;

    private readonly Asset<Texture2D> _hoveredBorderTexture;

    private readonly Asset<Texture2D> _iconTexture;

    private Color _color;

    private Color _borderColor;

    public float FadeFromBlack = 1f;

    private float _whiteLerp = 0.7f;

    private float _opacity = 0.7f;

    private bool _hovered;

    private bool _soundedHover;

    public readonly LocalizedText Description;

    private readonly UIText _title;

    private readonly Action<bool> _action;

    public float Priority;


    public bool IsSelected
    {
        get
        {
            return _enabled;
        }
    }

    public UIWorldGenToggle(Mod mod, LocalizedText title, LocalizedText description, Color textColor, string iconTexturePath, bool defaultValue, Action<bool> action, bool emode = false, float priority = 1f)
    {
        Mod = mod;
        _borderColor = Color.White;
        Description = description;
        _BasePanelTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/PanelGrayscale", (AssetRequestMode)1);
        _selectedBorderTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelHighlight", (AssetRequestMode)1);
        _hoveredBorderTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/CharCreation/CategoryPanelBorder", (AssetRequestMode)1);
        if (iconTexturePath != null)
        {
            _iconTexture = ModContent.Request<Texture2D>(iconTexturePath, (AssetRequestMode)1);
        }
        _color = Colors.InventoryDefaultColor;
        if (title != null)
        {
            UIText uIText = new(title, 0.9f)
            {
                HAlign = 0f,
                VAlign = 0.5f,
                Width = StyleDimension.FromPixelsAndPercent(0f - 10f, 1f),
                Top = StyleDimension.FromPixels(0f),
                Left = StyleDimension.FromPixelsAndPercent(0, 0.05f),
                TextColor = textColor
            };
            Append(uIText);
            _title = uIText;
        }
        _enabled = defaultValue;
        _action = action;

        if (emode)
        {
            EmodeTag emodeTag = new()
            {
                HAlign = 1f,
                VAlign = 0f,
                Width = StyleDimension.FromPixels(24),
                Height = StyleDimension.FromPixels(26)
            };
            Append(emodeTag);
        }

        Priority = priority;
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
        Color color = _color;
        float num = _opacity;
        bool isSelected = IsSelected;
        Utils.DrawSplicedPanel(spriteBatch, _BasePanelTexture.Value, (int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height, 10, 10, 10, 10, Color.Lerp(Color.Black, color, FadeFromBlack) * num);
        if (isSelected)
        {
            Utils.DrawSplicedPanel(spriteBatch, _selectedBorderTexture.Value, (int)dimensions.X + 7, (int)dimensions.Y + 7, (int)dimensions.Width - 14, (int)dimensions.Height - 14, 10, 10, 10, 10, Color.Lerp(color, Color.White, _whiteLerp) * num);
        }
        if (_hovered)
        {
            Utils.DrawSplicedPanel(spriteBatch, _hoveredBorderTexture.Value, (int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height, 10, 10, 10, 10, _borderColor);
        }
        if (_iconTexture != null)
        {
            var referenceTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/WorldCreation/IconEvilCorruption").Value;
            Color color2 = Color.White;
            Vector2 offset = (referenceTexture.Size() - _iconTexture.Value.Size()) / 2;
            spriteBatch.Draw(_iconTexture.Value, offset + new Vector2(dimensions.X + 1f, dimensions.Y + 1f), color2);
        }
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        _enabled = !_enabled;
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

    public void InvokeAction()
    {
        _action.Invoke(_enabled);
    }
}