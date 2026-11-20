using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace FargoSeeds;

[Label("Fargo Seeds Config")]
class WorldConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    public static WorldConfig Instance
    {
        get; private set;
    }

    public override void OnLoaded()
    {
        Instance = this;
    }

    [Header("$Mods.FargoSeeds.Configs.WorldConfig.HeaderToggles")]

    [Label("[i:86][i:1329] World Evil Toggle")]
    [DefaultValue(true)]
    public bool EvilToggle;

    [Label("[i:134] Dungeon Toggle")]
    [DefaultValue(true)]
    public bool DungeonToggle;

    [Label("[i:2625] Ocean Toggle")]
    [DefaultValue(true)]
    public bool OceanToggle;

    [Label("[i:276] Desert Toggle")]
    [DefaultValue(true)]
    public bool DesertToggle;

    [Label("[i:208] Jungle Toggle")]
    [DefaultValue(true)]
    public bool JungleToggle;

    [Label("[i:2358] Ice Toggle")]
    [DefaultValue(true)]
    public bool IceToggle;

    [Label("[i:2358] Caves Toggle")]
    [DefaultValue(true)]
    public bool CavesToggle;

    [Label("[i:183] Glowing Mushroom Toggle")]
    [DefaultValue(true)]
    public bool MushroomToggle;

    [Label("[i:318] Underworld Toggle")]
    [DefaultValue(true)]
    public bool UnderworldToggle;

    [Label("[i:5340] Aether Toggle")]
    [DefaultValue(true)]
    public bool AetherToggle;

    [Header("$Mods.FargoSeeds.Configs.WorldConfig.HeaderExtras")]
    [Label("[i:1169] Two Dungeons")]
    [DefaultValue(false)]
    public bool SecondDungeon;

    [Label("[i:3031][i:4820][i:5302][i:5364] Replace all Liquids With")]
    [DefaultValue(LiquidReplace.None)]
    [Slider]
    public LiquidReplace liquidReplace;


    [Label("[i:3335] Start in HardMode")]
    [DefaultValue(false)]
    public bool EarlyHardMode;

    [Label("[i:364] HardMode Ore")]
    [Increment(1)]
    [Range(0, 5)]
    [DefaultValue(0)]
    [Slider]
    public int HardModeOreMulti;

    [Label("[i:267] Spawn Location")]
    [DefaultValue(SpawnLocation.Normal)]
    [Slider]
    public SpawnLocation spawnLocation;

    [Label("Keep Spawn safe from both evils")] // Idk I don't have a good description rn change this later
    [DefaultValue(true)]
    public bool BothEvilsSpawnSafety;

    [Label("[i:267] Starting NPC")]
    [DefaultValue(StartingNPC.Guide)]
    [Slider]
    public StartingNPC startingNPC;

    //[Header("$Mods.FargoSeeds.Configs.WorldConfig.HeaderPost")]
    //[Label("[i:66] No Evil Spread")]
    //[DefaultValue(true)]
    //public bool NoEvilSpread;

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

    public enum LiquidReplace
    {
        None,
        Water,
        Lava,
        Honey,
        Shimmer,
        Random
    }
}