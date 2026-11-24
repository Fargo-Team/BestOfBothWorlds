using FargoSeeds.UI.WorldGenMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.NPC.NPCNameFakeLanguageCategoryPassthrough;

namespace FargoSeeds.Content;

public abstract class WorldGenOption(Mod mod, string name, string description, float priority)
{
    public Mod Mod = mod;

    public string Name = name;

    public string Description = description;

    public float Priority = priority;

    public object? Value { get; set; }

    public object? DefaultValue;

    public Asset<Texture2D>? Icon { get; init; }

    public abstract UIOptionElement ProvideElement();

    public abstract void Apply();
}

public class WorldGenOption<T> : WorldGenOption
{
    public Action<T>? OnApply;

    public WorldGenOption(Mod mod, string name, string description, float priority, T @default)
        : base(mod, name, description, priority)
    {
        DefaultValue = @default;
        Value = DefaultValue;
    }

    public override UIOptionElement ProvideElement()
    {
        return Value switch
        {
            bool => new ToggleOptionElement(this, Color.White),
            _ => new UIOptionElement(this, Color.White),
        };
    }

    public override void Apply()
    {
        OnApply?.Invoke((T)Value!);
    }
}

public sealed class RangeOption(Mod mod, string name, string description, float priority, float @default, float min, float max, int segments)
    : WorldGenOption<float>(mod, name, description, priority, @default)
{
    public override UIOptionElement ProvideElement()
    {
        return Value switch
        {
            float => new SliderOptionElement(this, min, max, segments, Color.White),
            _ => base.ProvideElement(),
        };
    }
}
