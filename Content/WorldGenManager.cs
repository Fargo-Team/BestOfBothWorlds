using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FargoSeeds.Common.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace FargoSeeds.Content;

public sealed class WorldGenManager : ILoadable
{
    public record OptionPreset(string Name, Dictionary<string, object> Options);

    public sealed class Category(string name, float priority)
    {
        public string Name = name;

        public float Priority = priority;
    }

    private static readonly List<OptionPreset> option_presets = [];

    public static readonly Dictionary<Category, List<WorldGenOption>> OPTIONS_BY_CATEGORY = [];

    public static void AddOption(WorldGenOption option, Category category)
    {
        OPTIONS_BY_CATEGORY.TryAdd(category, []);
        OPTIONS_BY_CATEGORY[category].Add(option);
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
        AddSliderOption(mod, name, description, category, evt, iconPath, priority, defaultValue, min, max, intSlider ? (int)(max - min) : -1);
    }

    public static void Reset()
    {
        foreach (var key in OPTIONS_BY_CATEGORY.Keys)
        {
            ResetCategory(key);
        }
    }

    public static void ResetCategory(Category category)
    {
        foreach (var option in OPTIONS_BY_CATEGORY[category])
        {
            option.Value = option.DefaultValue;
        }
    }

    private static void Apply()
    {
        foreach (var key in OPTIONS_BY_CATEGORY.Keys)
        foreach (var option in OPTIONS_BY_CATEGORY[key])
        {
            option.Apply();
        }
    }

    // I yearn for daybreak
    void ILoadable.Load(Mod mod)
    {
        On_WorldGen.CreateNewWorld += CreateNewWorld_ApplyOptions;

        LoadPresets();
    }

    void ILoadable.Unload()
    { }

    private static void LoadPresets()
    {
        Directory.CreateDirectory(FargoSeedsIO.SavePath);
        var files = Directory.GetFiles(FargoSeedsIO.SavePath);

        foreach (var path in files)
        {
            var name = Path.GetFileNameWithoutExtension(path);

            var json = File.ReadAllText(path);

            var options = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)!;

            var preset = new OptionPreset(name, options);

            option_presets.Add(preset);
        }
    }

    private static void SavePreset(OptionPreset preset)
    {
        var path = Path.Combine(FargoSeedsIO.SavePath, $"{preset.Name}.json");
        var json = JsonConvert.SerializeObject(preset.Options, Formatting.Indented);

        File.WriteAllText(path, json);
    }

    public static void ApplyPreset(string name)
    {
        if (option_presets.Find(p => p.Name == name) is not { } preset)
        {
            return;
        }

        foreach (var options in OPTIONS_BY_CATEGORY.Values)
        foreach (var opt in options)
        {
            opt.Value = opt.DefaultValue;

            if (preset.Options.TryGetValue(opt.Name, out var value))
            {
                opt.Value = value;
            }
        }
    }

    public static void AddPreset(string name)
    {
        var preset = new OptionPreset(name, []);

        foreach (var options in OPTIONS_BY_CATEGORY.Values)
        foreach (var opt in options)
        {
            if (opt.Value != opt.DefaultValue)
            {
                preset.Options.Add(opt.Name, opt.Value!);
            }
        }

        option_presets.Add(preset);

        SavePreset(preset);
    }

    private static Task CreateNewWorld_ApplyOptions(On_WorldGen.orig_CreateNewWorld orig, GenerationProgress progress)
    {
        Apply();

        return orig(progress);
    }
}
