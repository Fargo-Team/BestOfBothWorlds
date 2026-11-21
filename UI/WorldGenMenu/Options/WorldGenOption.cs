using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace FargoSeeds.UI.WorldGenMenu;

public class WorldGenOption : UIElement
{
    public Mod Mod;

    public object Value;

    public readonly LocalizedText Title;

    public readonly LocalizedText Description;

    public readonly float Priority;

    private readonly Asset<Texture2D>? icon;

    public WorldGenOption(
        Mod mod,
        LocalizedText title,
        LocalizedText description,
        float priority,
        Color textColor,
        string? iconTexturePath
    )
    {
        Mod = mod;

        Title = title;
        Description = description;

        Priority = priority;

        if (!string.IsNullOrEmpty(iconTexturePath))
        {
            icon = ModContent.Request<Texture2D>(iconTexturePath, (AssetRequestMode)1);
        }

        UIText label = new(title, 0.9f);
        {
            label.HAlign = 0f;
            label.VAlign = 0.5f;
            label.Width.Set(0f - 10f, 1f);
            label.Top.Set(0f, 0f);
            label.Left.Set(0, 0.05f);
            label.TextColor = textColor;
        }
        Append(label);
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
        var dims = this.Dimensions;

        Utils.DrawSplicedPanel(sb, texture, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, color);
    }

    protected void DrawIcon(SpriteBatch sb)
    {
        if (icon is null)
        {
            return;
        }

        var dims = this.Dimensions;

        var sizeRef = ModContent.Request<Texture2D>("Terraria/Images/UI/WorldCreation/IconEvilCorruption").Value;

        Color color2 = Color.White;
        Vector2 offset = (sizeRef.Size() - icon.Value.Size()) / 2;

        sb.Draw(icon.Value, offset + new Vector2(dims.X + 1f, dims.Y + 1f), color2);
    }

    public override int CompareTo(object? obj)
    {
        if (obj is not WorldGenOption opt)
        {
            return 0;
        }

        return Priority.CompareTo(opt);
    }

    public virtual void RunAction(object obj)
    { }
}

public class WorldGenOption<T> : WorldGenOption
{
    protected new T Value
    {
        get => (T)base.Value;
        set => base.Value = value!;
    }

    public Action<T>? OnRunAction;

    public WorldGenOption(
        Mod mod,
        LocalizedText title,
        LocalizedText description,
        float priority,
        T defaultValue,
        Color textColor,
        string? iconTexturePath
    ) : base(mod, title, description, priority, textColor, iconTexturePath)
    {
        Value = defaultValue;
    }

    public override void RunAction(object obj)
    {
        OnRunAction?.Invoke((T)obj);
    }
}
