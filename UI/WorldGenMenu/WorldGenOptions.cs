using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargoSeeds.UI.WorldGenMenu;

public class WorldGenOptions : ModSystem
{
    public const string KEY = "Mods.FargoSeeds.WorldGenMenu.";

    public override void Load()
    {
        AddOptions();
    }

    internal void AddToggle(LocalizedText category, string config, Color textColor, int iconItemID, bool defaultValue, Action<bool> action, bool emode = false)
    {
        string iconTexturePath = "Terraria/Images/Item_" + iconItemID;
        WorldGenUIManager.AddToggle(
            Mod,
            category,
            Language.GetText(KEY + config + ".Label"),
            Language.GetText(KEY + config + ".Tooltip"),
            textColor,
            iconTexturePath,
            defaultValue,
            action,
            emode
        );
    }
    internal void AddToggle(LocalizedText category, string config, Color textColor, string iconTexturePath, bool defaultValue, Action<bool> action, bool emode = false)
    {
        WorldGenUIManager.AddToggle(
            Mod,
            category,
            Language.GetText(KEY + config + ".Label"),
            Language.GetText(KEY + config + ".Tooltip"),
            textColor,
            iconTexturePath,
            defaultValue,
            action,
            emode
        );
    }
    internal void AddSlider(LocalizedText category, string config, Color textColor, string iconTexturePath, float defaultValue, Action<float> action, bool intSlider, float min, float max)
    {
        WorldGenUIManager.AddSlider(
            Mod,
            category,
            Language.GetText(KEY + config + ".Label"),
            Language.GetText(KEY + config + ".Tooltip"),
            textColor,
            iconTexturePath,
            defaultValue,
            action,
            intSlider,
            min,
            max
        );
    }
    internal void AddFrequencySlider(string config, Color textColor, int iconItemID, Action<float> action, int defaultValue = 1)
    {
        string iconTexturePath = "Terraria/Images/Item_" + iconItemID;
        WorldGenUIManager.AddSlider(
            Mod,
            Language.GetText(KEY + "HeaderFrequency"),
            Language.GetText(KEY + config + ".Label"),
            Language.GetText(KEY + config + ".Tooltip"),
            textColor,
            iconTexturePath,
            defaultValue,
            action,
            true,
            0,
            5
        );
    }
    internal void AddOptions()
    {
        string headerPath = KEY + "Header";
        bool souls = ModLoader.HasMod("FargowiltasSouls");

        Color color = Color.White;

        // NEW FEATURES
        LocalizedText header = Language.GetText(headerPath + "NewFeatures");
        AddToggle(header, "FishingShacks", color, ItemID.FishingBobber, souls, (value) => { FishingShacks = value; }, emode: true);
        AddToggle(header, "GrandCaverns", color, ItemID.Boulder, souls, (value) => { GrandCaverns = value; }, emode: true);
        AddToggle(header, "Mineshafts", color, ItemID.Rope, souls, (value) => { Mineshafts = value; }, emode: true);
        AddToggle(header, "Graveyard", color, ItemID.Tombstone, souls, (value) => { Graveyard = value;  }, emode: true);

        // OVERHAULS
        header = Language.GetText(headerPath + "Overhauls");
        AddToggle(header, "BothEvils", color, ItemID.CrimstoneBlock, true, (value) => { BothEvils = value; }, emode: true);
        AddToggle(header, "BothOres", color, ItemID.TinOre, true, (value) => { BothOres = value; }, emode: true);
        AddToggle(header, "WavyCaves", color, ItemID.StoneBlock, souls, (value) => { WavyCaves = value; }, emode: true);
        AddToggle(header, "RemixUnderworld", color, ItemID.Hellstone, souls, (value) => { RemixUnderworld = value; }, emode: true);

        // VANILLA FEATURES
        header = Language.GetText(headerPath + "VanillaFeatures");
        AddToggle(header, "BigHives", color, ItemID.Hive, souls, (value) => { BigHives = value; }, emode: true);
        AddToggle(header, "PyramidEntrance", color, 848, souls, (value) => { PyramidEntrance = value; }, emode: true);
        AddToggle(header, "OceanCaves", color, ItemID.Coral, false, (value) => { OceanCaves = value; });
        AddToggle(header, "LivingTrees", color, 832, false, (value) => { LivingTrees = value; });
        AddToggle(header, "SurfaceMushroom", color, ItemID.GlowingMushroom, false, (value) => { SurfaceMushroom = value; });
        AddToggle(header, "SurfaceMarble", color, ItemID.MarbleBlock, false, (value) => { SurfaceMarble = value; });
        AddToggle(header, "SurfaceSpiders", color, ItemID.Cobweb, false, (value) => { SurfaceSpiders = value; });
        AddToggle(header, "HugeTemple", color, 1153, false, (value) => { HugeTemple = value; });
        AddToggle(header, "SecondShimmer", color, ItemID.BottomlessShimmerBucket, false, (value) => { SecondShimmer = value; });

        // FREQUENCY
        AddFrequencySlider("CaveMultiplier", color, ItemID.StoneBlock, (value) => { CaveMultiplier = (int)value; });
        AddFrequencySlider("TreeMultiplier", color, 4765, (value) => { TreeMultiplier = (int)value; });
        AddFrequencySlider("FloatingIslandMultiplier", color, 838, (value) => { FloatingIslandMultiplier = (int)value; });
        AddFrequencySlider("MarbleMultiplier", color, ItemID.Marble, (value) => { MarbleMultiplier = (int)value; });
        AddFrequencySlider("GraniteMultiplier", color, ItemID.Granite, (value) => { GraniteMultiplier = (int)value; });
        AddFrequencySlider("SpiderMultiplier", color, 952, (value) => { SpiderMultiplier = (int)value; });
        AddFrequencySlider("HiveMultiplier", color, ItemID.Hive, (value) => { HiveMultiplier = (int)value; });
        AddFrequencySlider("OreMultiplier", color, 13, (value) => { OreMultiplier = (int)value; });
        AddFrequencySlider("GemMultiplier", color, ItemID.Diamond, (value) => { GemMultiplier = (int)value; });
        AddFrequencySlider("ChestMultiplier", color, 306, (value) => { ChestMultiplier = (int)value; });
        AddFrequencySlider("PotMultiplier", color, ItemID.ObsidianVase, (value) => { PotMultiplier = (int)value; });
        AddFrequencySlider("LifeCrystalMultiplier", color, 29, (value) => { LifeCrystalMultiplier = (int)value; });
        AddFrequencySlider("StatueMultiplier", color, 52, (value) => { StatueMultiplier = (int)value; });
        AddFrequencySlider("TrapMultiplier", color, 539, (value) => { TrapMultiplier = (int)value; });
        AddFrequencySlider("MicroMultiplier", color, ItemID.MinecartTrack, (value) => { MicroMultiplier = (int)value; });

        // EXPERIMENTAL
        header = Language.GetText(headerPath + "Experimental");
        AddToggle(header, "SurfaceTemple", color, 1153, false, (value) => { SurfaceTemple = value; });
        AddToggle(header, "EvilSurface", color, 59, false, (value) => { EvilSurface = value; });
        AddToggle(header, "NoTraps", color, ItemID.DartTrap, false, (value) => { NoTraps = value; });
        AddToggle(header, "MushroomLayer", color, 183, false, (value) => { MushroomLayer = value; });
        AddToggle(header, "RainbowCabins", color, ItemID.Paintbrush, false, (value) => { RainbowCabins = value; });
        AddToggle(header, "PaintEverything", color, ItemID.Paintbrush, false, (value) => { PaintEverything = value; });
        AddToggle(header, "MahoganyTrees", color, 3360, false, (value) => { MahoganyTrees = value; });
        AddToggle(header, "UndergroundDungeon", color, ItemID.BlueBrick, false, (value) => { UndergroundDungeon = value; });
        AddToggle(header, "MarbleGraniteSwapped", color, ItemID.Marble, false, (value) => { MarbleGraniteSwapped = value; });
    }

#region New Features
    public static bool Mineshafts;
    public static bool GrandCaverns;
    public static bool FishingShacks;
    public static bool Graveyard;
#endregion

#region Features and Overhauls
    public static bool BothEvils;

    public static bool BothOres;

    public static bool WavyCaves;

    public static bool RemixUnderworld;

    public static bool LivingTrees;

    public static bool SurfaceMushroom;

    public static bool SurfaceMarble;

    public static bool SurfaceSpiders;

    public static bool BigHives;

    public static bool PyramidEntrance;

    public static bool HugeTemple;

    public static bool SecondShimmer;
#endregion

#region Frequency
    // all of these are 0-5 int
    public static int TreeMultiplier;

    public static int CaveMultiplier;

    public static int FloatingIslandMultiplier;

    public static int MarbleMultiplier;

    public static int GraniteMultiplier;

    public static int SpiderMultiplier;

    public static int HiveMultiplier;

    public static int OreMultiplier;

    public static int GemMultiplier;

    public static int ChestMultiplier;

    public static int PotMultiplier;

    public static int LifeCrystalMultiplier;

    public static int StatueMultiplier;

    public static int TrapMultiplier;

    public static int MicroMultiplier;
#endregion

#region Experimental

    public static bool OceanCaves;

    public static bool SurfaceTemple;

    public static bool EvilSurface;

    public static bool NoTraps;

    public static bool MushroomLayer;

    public static bool RainbowCabins;

    public static bool PaintEverything;

    public static bool MahoganyTrees;

    public static bool UndergroundDungeon;

    public static bool MarbleGraniteSwapped;
#endregion

    /*

    [Label("[i:267] Spawn Location")]
    [DefaultValue(SpawnLocation.Normal)]
    [Slider]
    public SpawnLocation spawnLocation;


    [Label("[i:267] Starting NPC")]
    [DefaultValue(StartingNPC.Guide)]
    [Slider]
    public StartingNPC startingNPC;


    public enum SpawnLocation
    {
        Normal,
        Ocean,
        Underworld,
        Random
    }

    public enum StartingNPC
    {
        Guide,
        PartyGirl,
        Merchant,
        Demolitionist,
        PartyGroup,
        TaxCollector,
        Angler
    }
    */
}