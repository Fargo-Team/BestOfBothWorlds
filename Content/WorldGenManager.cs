using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace FargoSeeds.Content;

public sealed class WorldGenManager : ILoadable
{
    public record Category(string Name, float Priority);

    private static readonly Dictionary<Category, List<WorldGenOption>> options_by_category = [];

    public static void AddOption(WorldGenOption option, Category category)
    {
        options_by_category.TryAdd(category, []);
        options_by_category[category].Add(option);
    }

    public static void AddToggleOption(Mod mod, string name, string description, Category category, Action<bool> evt, string iconPath = "", float priority = 1f, bool defaultValue = false)
    {
        AddOption(
            new WorldGenOption<bool>(mod, name, description, priority, defaultValue)
            {
                OnApply = evt,
                Icon = string.IsNullOrEmpty(iconPath) ? null : ModContent.Request<Texture2D>(iconPath)
            },
            category
        );
    }

    public static void AddSliderOption(Mod mod, string name, string description, Category category, Action<float> evt, string iconPath = "", float priority = 1f, float defaultValue = 0f, float min = 0f, float max = 1f, int segments = -1)
    {
        AddOption(
            new RangeOption(mod, name, description, priority, defaultValue, min, max, segments)
            {
                OnApply = evt,
                Icon = string.IsNullOrEmpty(iconPath) ? null : ModContent.Request<Texture2D>(iconPath)
            },
            category
        );
    }

    public static void AddSliderOption(Mod mod, string name, string description, Category category, Action<float> evt, string iconPath = "", float priority = 1f, float defaultValue = 0f, float min = 0f, float max = 1f, bool intSlider = false)
    {
        AddSliderOption(mod, name, description, category, evt, iconPath, priority, defaultValue, min, max, intSlider ? -1 : (int)(max - min));
    }

    public static void Reset()
    {
        foreach (var key in options_by_category.Keys)
        {
            ResetCategory(key);
        }
    }

    public static void ResetCategory(Category category)
    {
        foreach (var option in options_by_category[category])
        {
            option.Value = option.DefaultValue;
        }
    }

    private static void Apply()
    {
        foreach (var key in options_by_category.Keys)
        foreach (var option in options_by_category[key])
        {
            option.Apply();
        }
    }

    // I yearn for daybreak
    void ILoadable.Load(Mod _)
    {
        On_WorldGen.CreateNewWorld += CreateNewWorld_ApplyOptions;
    }

    void ILoadable.Unload()
    { }

    private static Task CreateNewWorld_ApplyOptions(On_WorldGen.orig_CreateNewWorld orig, GenerationProgress progress)
    {
        Apply();

        return orig(progress);
    }
}
