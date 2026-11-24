using FargoSeeds.UI.WorldGenMenu;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using FargoSeeds.Content;
using Terraria;
using Terraria.ModLoader;

namespace FargoSeeds;

public class FargoSeeds : Mod
{
    public static Mod Mod;

    public override void Load()
    {
        Mod = this;
    }

    public override object Call(params object[] args)
    {
        if (Main.dedServ)
        {
            return null!;
        }

        try
        {
            var @case = args[0] as string;

            switch (@case)
            {
                case "AddToggle":
                {
                    WorldGenManager.AddToggleOption(
                        (Mod)args[1],                 // Calling Mod
                        (string)args[2],              // Name (Localization Key)
                        (string)args[3],              // Description (Localization Key)
                        GetCategory((string)args[4]), // Category (Localization Key) (One will be created if the category is not found)
                        (Action<bool>)args[5],        // Application event
                        (string)args[6],              // Icon Path
                        (float)args[7],               // Priority
                        (bool)args[8]                 // Default value
                    );
                }
                break;

                case "AddSlider":
                {
                    WorldGenManager.AddSliderOption(
                        (Mod)args[1],                 // Calling Mod
                        (string)args[2],              // Name (Localization Key)
                        (string)args[3],              // Description (Localization Key)
                        GetCategory((string)args[4]), // Category (Localization Key) (One will be created if the category is not found)
                        (Action<float>)args[5],       // Application event
                        (string)args[6],              // Icon Path
                        (float)args[7],               // Priority
                        (float)args[8],               // Default value
                        (float)args[9],               // Min
                        (float)args[10],              // Max
                        (int)args[11]                 // Steps (how many 'ticks' the slider has (-1 if smooth))
                    );
                }
                break;

                case "ChangeCategoryPriority":
                {
                    var categoryKey = (string)args[1];

                    if (WorldGenManager.OPTIONS_BY_CATEGORY.Keys.ToList().Find(c => c.Name == categoryKey) is not { } category)
                    {
                        throw new KeyNotFoundException("Category was not present!");
                    }

                    category.Priority = (float)args[2];
                }
                break;
            }
        }
        catch (Exception e)
        {
            Logger.Error("Mod.Call Error: " + e.StackTrace + e.Message);
        }

        return null!;

        static WorldGenManager.Category GetCategory(string categoryKey)
        {
            if (WorldGenManager.OPTIONS_BY_CATEGORY.Keys.ToList().Find(c => c.Name == categoryKey) is not { } category)
            {
                category = new WorldGenManager.Category(categoryKey, 1f);
            }

            return category;
        }
    }
}