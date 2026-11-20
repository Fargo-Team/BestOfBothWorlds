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
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public class EmodeTag : UIElement
{
    private readonly Asset<Texture2D> _Texture;

    // Should only exist if Souls Mod is enabled
    public EmodeTag()
    {
        _Texture = ModContent.Request<Texture2D>("FargoSeeds/UI/WorldGenMenu/EmodeTag", (AssetRequestMode)1);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (!ModLoader.TryGetMod("FargowiltasSouls", out Mod SoulsMod))
        {
            return;
        }
        CalculatedStyle dimensions = GetDimensions();
        if (IsMouseHovering)
        {
            string text = Language.GetTextValue("Mods.FargoSeeds.WorldGenMenu.EmodeTag");

            UICommon.TooltipMouseText(text);
        }

        Color color = Color.White;
        spriteBatch.Draw(_Texture.Value, dimensions.Center(), null, color, 0, _Texture.Value.Size() / 2, 0.75f, SpriteEffects.None, 0);
    }
}