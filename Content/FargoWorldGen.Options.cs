using System;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargoSeeds.Content;

public sealed class FargoWorldGenOptions : ModSystem
{
    public const string KEY = "Mods.FargoSeeds.WorldGenMenu.";

    public override void Load()
    {
        AddOptions();
    }

    internal void AddOptions()
    {
        const string header_path = KEY + "Header.";

        var souls = ModLoader.HasMod("FargowiltasSouls");

        // TODO: Parse these from the file instead.
        var category = new WorldGenManager.Category(header_path + "NewFeatures", 0f);
        AddBool(nameof(FishingShacks), ItemID.FishingBobber, souls, value => { FishingShacks = value; }); // emode
        AddBool(nameof(GrandCaverns), ItemID.Boulder, souls, value => { GrandCaverns = value; });         // emode
        AddBool(nameof(Mineshafts), ItemID.Rope, souls, value => { Mineshafts = value; });                // emode
        AddBool(nameof(Graveyard), ItemID.Tombstone, souls, value => { Graveyard = value; });             // emode

        // OVERHAULS
        category = new WorldGenManager.Category(header_path + "Overhauls", 1f);
        AddBool(nameof(BothEvils), ItemID.CrimstoneBlock, true, value => { BothEvils = value; });         // emode
        AddBool(nameof(BothOres), ItemID.TinOre, true, value => { BothOres = value; } );                  // emode
        AddBool(nameof(WavyCaves), ItemID.StoneBlock, souls, value => { WavyCaves = value; });            // emode
        AddBool(nameof(RemixUnderworld), ItemID.Hellstone, souls, value => { RemixUnderworld = value; }); // emode

        category = new WorldGenManager.Category(header_path + "VanillaFeatures", 2);
        AddBool(nameof(BigHives), ItemID.Hive, souls, value => { BigHives = value; });                    // emode
        AddBool(nameof(PyramidEntrance), 848, souls, value => { PyramidEntrance = value; });              // emode
        AddBool(nameof(OceanCaves), ItemID.Coral, false, value => { OceanCaves = value; });
        AddBool(nameof(LivingTrees), 832, false, value => { LivingTrees = value; });
        AddBool(nameof(SurfaceMushroom), ItemID.GlowingMushroom, false, value => { SurfaceMushroom = value; });
        AddBool(nameof(SurfaceMarble), ItemID.MarbleBlock, false, value => { SurfaceMarble = value; });
        AddBool(nameof(SurfaceSpiders), ItemID.Cobweb, false, value => { SurfaceSpiders = value; });
        AddBool(nameof(HugeTemple), 1153, false, value => { HugeTemple = value; });
        AddBool(nameof(SecondShimmer), ItemID.BottomlessShimmerBucket, false, value => { SecondShimmer = value; });

        AddFreq(nameof(CaveMultiplier), ItemID.StoneBlock, value => { CaveMultiplier = (int)value; });
        AddFreq(nameof(TreeMultiplier), 4765, value => { TreeMultiplier = (int)value; });
        AddFreq(nameof(FloatingIslandMultiplier), 838, value => { FloatingIslandMultiplier = (int)value; });
        AddFreq(nameof(MarbleMultiplier), ItemID.Marble, value => { MarbleMultiplier = (int)value; });
        AddFreq(nameof(GraniteMultiplier), ItemID.Granite, value => { GraniteMultiplier = (int)value; });
        AddFreq(nameof(SpiderMultiplier), 952, value => { SpiderMultiplier = (int)value; });
        AddFreq(nameof(HiveMultiplier), ItemID.Hive, value => { HiveMultiplier = (int)value; });
        AddFreq(nameof(OreMultiplier), 13, value => { OreMultiplier = (int)value; });
        AddFreq(nameof(GemMultiplier), ItemID.Diamond, value => { GemMultiplier = (int)value; });
        AddFreq(nameof(ChestMultiplier), 306, value => { ChestMultiplier = (int)value; });
        AddFreq(nameof(PotMultiplier), ItemID.ObsidianVase, value => { PotMultiplier = (int)value; });
        AddFreq(nameof(LifeCrystalMultiplier), 29, value => { LifeCrystalMultiplier = (int)value; });
        AddFreq(nameof(StatueMultiplier), 52, value => { StatueMultiplier = (int)value; });
        AddFreq(nameof(TrapMultiplier), 539, value => { TrapMultiplier = (int)value; });
        AddFreq(nameof(MicroMultiplier), ItemID.MinecartTrack, value => { MicroMultiplier = (int)value; });

        category = new WorldGenManager.Category(header_path + "Experimental", 3f);
        AddBool(nameof(SurfaceTemple), 1153, false, value => { SurfaceTemple = value; });
        AddBool(nameof(EvilSurface), 59, false, value => { EvilSurface = value; });
        AddBool(nameof(NoTraps), ItemID.DartTrap, false, value => { NoTraps = value; });
        AddBool(nameof(MushroomLayer), 183, false, value => { MushroomLayer = value; });
        AddBool(nameof(RainbowCabins), ItemID.Paintbrush, false, value => { RainbowCabins = value; });
        AddBool(nameof(PaintEverything), ItemID.Paintbrush, false, value => { PaintEverything = value; });
        AddBool(nameof(MahoganyTrees), 3360, false, value => { MahoganyTrees = value; });
        AddBool(nameof(UndergroundDungeon), ItemID.BlueBrick, false, value => { UndergroundDungeon = value; });
        AddBool(nameof(MarbleGraniteSwapped), ItemID.Marble, false, value => { MarbleGraniteSwapped = value; });

        return;

        void AddBool(string name, int itemIconId, bool @default, Action<bool> evt)
        {
            WorldGenManager.AddToggleOption(
                Mod,
                KEY + name + ".Label",
                KEY + name + ".Tooltip",
                category,
                evt,
                "Terraria/Images/Item_" + itemIconId,
                1f,
                @default
            );
        }

        void AddFreq(string name, int itemIconId, Action<float> evt)
        {
            WorldGenManager.AddSliderOption(
                Mod,
                KEY + name + ".Label",
                KEY + name + ".Tooltip",
                category,
                evt,
                "Terraria/Images/Item_" + itemIconId,
                1f,
                0f,
                0f,
                5f,
                true
            );
        }
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