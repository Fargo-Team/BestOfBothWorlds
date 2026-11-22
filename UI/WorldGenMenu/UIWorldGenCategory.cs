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

public class UIWorldGenCategory : UIElement
{
    private LocalizedText _titleText;
    private UIText _title;
    public float Priority;
    public LocalizedText Text
    {
        get 
        { 
            return _titleText; 
        }
    }
    public UIWorldGenCategory(LocalizedText title, float priority)
    {
        this.Width.Set(1f, 0);
        this.Height.Set(1f, 0);
        if (title != null)
        {
            _titleText = title;
            UIText uIText = new(title, 1f)
            {
                HAlign = 0f,
                VAlign = 0.5f,
                Width = StyleDimension.FromPixelsAndPercent(0f - 10f, 1f),
                Top = StyleDimension.FromPixels(0f),
                TextColor = Color.White
            };
            Append(uIText);
            _title = uIText;

            UIHorizontalSeparator separator = new()
            {
                Width = StyleDimension.FromPercent(1f),
                Top = StyleDimension.FromPercent(0.9f),
                Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
            };
            Append(separator);
        }

        Priority = priority; 
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {

    }

}