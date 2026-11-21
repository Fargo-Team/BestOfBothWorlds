using FargoSeeds.UI.WorldGenMenu;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargoSeeds;

public class FargoSeeds : Mod
{
    public static Mod Mod;

    public override void Load()
    {
        Mod = this;
    }

    // Zoey: Unsure how much I can mess with ModCall stuff; I've opted to not adjust the args, but do not Priority
    // is available for any element (also make the float range not a list???? hello??? like even if for some reason
    // you wanted this we HAVE Terraria.Utilities.Terraria.Utilities.FloatRange,) please do improve this.
    public override object Call(params object[] args)
    {
        if (Main.dedServ)
        {
            return null!;
        }

        try
        {
            string code = args[0].ToString();

            switch (code)
            {
                case "AddWorldGenToggle":
                {
                    if (args[1].GetType() != typeof(string)) // Your mod name
                        break;
                    if (args[2].GetType() != typeof(string)) // Toggle Category
                        break;
                    if (args[3].GetType() != typeof(string)) // Toggle Title
                        break;
                    if (args[4].GetType() != typeof(string)) // Toggle Description
                        break;
                    if (args[5].GetType() != typeof(Color)) // Toggle title text color
                        break;
                    if (args[6].GetType() != typeof(string)) // Toggle icon texture path
                        break;
                    if (args[7].GetType() != typeof(bool)) // Default toggle value
                        break;
                    if (args[8].GetType() != typeof(Action<bool>)) // Toggle action (should flip your bool that determines the given worldgen option
                        break;
                    // optional arg
                    bool emode = false;
                    float priority = 1f;
                    if (args.Length > 9)
                    {
                        if (args[9].GetType() != typeof(bool)) // Whether this should be recommended for Eternity Mode
                            break;
                        emode = (bool)args[9];

                        if (args.Length > 10)
                        {
                            if (args[10].GetType() != typeof(float)) // Priority
                                break;
                            priority = 1f;
                        }
                    }
                    WorldGenUIManager.AddToggle(ModLoader.GetMod((string)args[1]), Language.GetText((string)args[2]), Language.GetText((string)args[3]), Language.GetText((string)args[4]), (Color)args[5], (string)args[6], (bool)args[7], (Action<bool>)args[8], emode, priority);
                }
                break;
                case "AddWorldGenSlider":
                {
                    if (args[1].GetType() != typeof(string)) // Your mod name
                        break;
                    if (args[2].GetType() != typeof(string)) // Toggle Category
                        break;
                    if (args[3].GetType() != typeof(string)) // Toggle Title
                        break;
                    if (args[4].GetType() != typeof(string)) // Toggle Description
                        break;
                    if (args[5].GetType() != typeof(Color)) // Toggle title text color
                        break;
                    if (args[6].GetType() != typeof(string)) // Toggle icon texture path
                        break;
                    if (args[7].GetType() != typeof(float)) // Default slider value
                        break;
                    if (args[8].GetType() != typeof(Action<float>)) // Toggle action (should flip your bool that determines the given worldgen option
                        break;
                    if (args[9].GetType() != typeof(Action<float>)) // Whether the slider should be locked to integers
                        break;
                    if (args[10].GetType() != typeof(List<float>)) // Slider range
                        break;
                    if (((List<float>)args[9]).Count != 2) // Range must have two elements (min, max)
                        break;

                    var list = (List<float>)args[10];

                    WorldGenUIManager.AddSlider(ModLoader.GetMod((string)args[1]), Language.GetText((string)args[2]), Language.GetText((string)args[3]), Language.GetText((string)args[4]), (Color)args[5], (string)args[6], (float)args[7], (Action<float>)args[8], (bool)args[9], list[0], list[1]);
                }
                break;
                case "SetCategoryPriority":
                {
                    if (args[1].GetType() != typeof(string)) // Your mod name
                        break;
                    if (args[2].GetType() != typeof(string)) // Toggle Category
                        break;
                    if (args[3].GetType() != typeof(float)) // The new priority
                        break;
                    WorldGenUIManager.ChangeCategoryPriority(Language.GetText((string)args[2]), (float)args[3]);
                }
                break;
            }

        }
        catch (Exception e)
        {
            Logger.Error("Call Error: " + e.StackTrace + e.Message);
        }

        return null!;
    }
}