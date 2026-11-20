using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public class UIWorldGenSlider : UIElement
{
    private static UIWorldGenSlider rightLock;
    private static UIWorldGenSlider rightHover;

    private float _sliderValue;

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

    public readonly LocalizedText Title;

    public readonly LocalizedText Description;

    private readonly UIText _title;

    private readonly Action<float> _action;

    private readonly bool _intSlider;

    private readonly float _sliderMin;
    private readonly float _sliderMax;

    public UIWorldGenSlider(Mod mod, LocalizedText title, LocalizedText description, Color textColor, string iconTexturePath, float defaultValue, Action<float> action, bool intSlider, List<float> sliderRange)
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
            Title = title;
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
        _action = action;
        _intSlider = intSlider;
        _sliderMin = sliderRange[0];
        _sliderMax = sliderRange[1];

        _sliderValue = (defaultValue - _sliderMin) / (_sliderMax - _sliderMin);
        _sliderValue = MathHelper.Clamp(_sliderValue, 0, 1);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        float num = 6f;
        int num2 = 0;

        rightHover = null;

        if (!Main.mouseLeft)
        {
            rightLock = null;
        }

        if (rightLock == this)
        {
            num2 = 1;
        }
        else if (rightLock != null)
        {
            num2 = 2;
        }

        CalculatedStyle dimensions = GetDimensions();
        Color color = _color;
        Utils.DrawSplicedPanel(spriteBatch, _BasePanelTexture.Value, (int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height, 10, 10, 10, 10, Color.Lerp(Color.Black, color, FadeFromBlack) * _opacity);

        if (_iconTexture != null)
        {
            var referenceTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/WorldCreation/IconEvilCorruption").Value;
            Color color2 = Color.White;
            Vector2 offset = (referenceTexture.Size() - _iconTexture.Value.Size()) / 2;
            spriteBatch.Draw(_iconTexture.Value, offset + new Vector2(dimensions.X + 1f, dimensions.Y + 1f), color2);
        }

        float num3 = dimensions.Width + 1f;
        Vector2 vector = new Vector2(dimensions.X, dimensions.Y);
        bool flag2 = IsMouseHovering;

        if (num2 == 1)
        {
            flag2 = true;
        }

        if (num2 == 2)
        {
            flag2 = false;
        }

        Vector2 vector2 = vector;
        vector2.X += 8f;
        vector2.Y += 2f + num;
        vector2.X -= 17f;
        //TextureAssets.ColorBar.Value.Frame(1, 1, 0, 0);
        vector2 = new Vector2(dimensions.X + dimensions.Width - 10f, dimensions.Y + 10f + num);
        IngameOptions.valuePosition = vector2;
        float obj = DrawValueBar(spriteBatch, 1f, _sliderValue, num2);

        if (IngameOptions.inBar || rightLock == this)
        {
            rightHover = this;
            if (PlayerInput.Triggers.Current.MouseLeft && rightLock == this)
            {
                _sliderValue = obj;
            }
        }

        if (rightHover != null && rightLock == null && PlayerInput.Triggers.JustPressed.MouseLeft)
        {
            rightLock = rightHover;
        }

        _title.SetText(Title.Value + ": " + MathHelper.Lerp(_sliderMin, _sliderMax, _sliderValue));
    }
    public float DrawValueBar(SpriteBatch sb, float scale, float perc, int lockState = 0)
    {
        perc = Utils.Clamp(perc, -.05f, 1.05f);

        Utils.ColorLerpMethod colorMethod = new Utils.ColorLerpMethod(Utils.ColorLerp_BlackToWhite);

        Texture2D colorBarTexture = TextureAssets.ColorBar.Value;
        Vector2 vector = new Vector2((float)colorBarTexture.Width, (float)colorBarTexture.Height * 1.5f) * scale;
        IngameOptions.valuePosition.X -= (float)((int)vector.X);
        Rectangle rectangle = new Rectangle((int)IngameOptions.valuePosition.X, (int)IngameOptions.valuePosition.Y - (int)vector.Y / 2 + 2, (int)vector.X, (int)vector.Y);
        Rectangle destinationRectangle = rectangle;
        int num = 167;
        float num2 = rectangle.X + 5f * scale;
        float num3 = rectangle.Y + 4f * scale;

        /*
        if (_intSlider)
        {
            int numTicks = (int)_sliderMax - (int)_sliderMin;
            if (numTicks > 1)
            {
                for (int tick = 0; tick < numTicks; tick++)
                {
                    float percent = (float)tick / numTicks;

                    if (percent <= 1f)
                        sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)(num2 + num * percent * scale), rectangle.Y - 2, 2, rectangle.Height + 4), Color.White);
                }
            }
        }
        */

        sb.Draw(colorBarTexture, rectangle, _color);

        for (float num4 = 0f; num4 < (float)num; num4 += 1f)
        {
            float percent = num4 / (float)num;
            //sb.Draw(TextureAssets.ColorBlip.Value, new Vector2(num2 + num4 * scale, num3), null, colorMethod(percent), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        rectangle.Inflate((int)(-5f * scale), 2);

        //rectangle.X = (int)num2;
        //rectangle.Y = (int)num3;

        bool flag = rectangle.Contains(new Point(Main.mouseX, Main.mouseY));

        if (lockState == 2)
        {
            flag = false;
        }

        if (flag || lockState == 1)
        {
            sb.Draw(TextureAssets.ColorHighlight.Value, destinationRectangle, Main.OurFavoriteColor);
        }

        var colorSlider = TextureAssets.ColorSlider.Value;

        sb.Draw(colorSlider, new Vector2(num2 + 167f * scale * perc, num3 + 8f * scale), null, Color.White, 0f, colorSlider.Size() * 0.5f, scale, SpriteEffects.None, 0f);

        if (Main.mouseX >= rectangle.X && Main.mouseX <= rectangle.X + rectangle.Width)
        {
            IngameOptions.inBar = flag;
            float obj = (Main.mouseX - rectangle.X) / (float)rectangle.Width;
            if (_intSlider)
            {
                int numTicks = (int)_sliderMax - (int)_sliderMin;
                obj = MathF.Round(obj * numTicks) / numTicks;
            }
            return obj;
        }

        IngameOptions.inBar = false;

        if (rectangle.X >= Main.mouseX)
        {
            return 0f;
        }

        return 1f;
    }

        

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
        //_enabled = !_enabled;
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
        _action.Invoke(MathHelper.Lerp(_sliderMin, _sliderMax, _sliderValue));
    }
}