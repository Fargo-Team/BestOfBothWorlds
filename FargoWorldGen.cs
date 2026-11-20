using FargoSeeds.UI.WorldGenMenu;
using FargoSeeds.WorldGeneration;
using Iced.Intel;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static System.Net.Mime.MediaTypeNames;
using static Terraria.GameContent.Animations.Actions.Sprites;
using static tModPorter.ProgressUpdate;

namespace FargoSeeds;

class FargoWorldGen : ModSystem
{
    bool drunkSeed = false;
    bool celebrationSeed = false;
    bool ftwSeed = false;
    bool beeSeed = false;
    bool starveSeed = false;
    bool remixSeed = false;
    bool trapSeed = false;
    bool gfbSeed = false;

    public override void PreWorldGen()
    {
        if (WorldGen.drunkWorldGen)
        {
            drunkSeed = true;
        }
        if (WorldGen.notTheBees)
        {
            beeSeed = true;
        }
        if (WorldGen.remixWorldGen)
        {
            remixSeed = true;
        }
        if (WorldGen.noTrapsWorldGen)
        {
            trapSeed = true;
        }
        if (WorldGen.dontStarveWorldGen)
        {
            starveSeed = true;
        }
        if (WorldGen.getGoodWorldGen)
        {
            ftwSeed = true;
        }
        if (WorldGen.tenthAnniversaryWorldGen)
        {
            celebrationSeed = true;
        }
        if (WorldGen.everythingWorldGen)
        {
            gfbSeed = true;
        }
    }
    // testing method
    public override void PostUpdatePlayers()
    {
        /*
        if (Main.mouseLeft && Main.mouseLeftRelease)
        {
            Point gravePointer = Main.MouseWorld.ToTileCoordinates();
            NewFeatures.TryPlaceGraveyard(gravePointer);
        }
        */
    }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        ///////////////steps in reverse order so additions dont get too whacky//////
        ///

        //EXTRAS
        int finalCleanupIndex = getStepIndex(tasks, "Final Cleanup");

        if (WorldGenOptions.SecondShimmer)
        {
            tasks.Insert(finalCleanupIndex, new PassLegacy("Second Shimmer", secondShimmer));
            finalCleanupIndex = getStepIndex(tasks, "Final Cleanup");
        }

        switch (WorldConfig.Instance.liquidReplace)
        {
            case WorldConfig.LiquidReplace.Water:
                tasks.Insert(finalCleanupIndex, new PassLegacy("All Water", replaceLiquidWithWater));
            break;

            case WorldConfig.LiquidReplace.Lava:
                tasks.Insert(finalCleanupIndex, new PassLegacy("All Lava", replaceLiquidWithLava));
            break;

            case WorldConfig.LiquidReplace.Honey:
                tasks.Insert(finalCleanupIndex, new PassLegacy("All Honey", replaceLiquidWithHoney));
            break;

            case WorldConfig.LiquidReplace.Shimmer:
                tasks.Insert(finalCleanupIndex, new PassLegacy("All Shimmer", replaceLiquidWithShimmer));
            break;

            case WorldConfig.LiquidReplace.Random:
                tasks.Insert(finalCleanupIndex, new PassLegacy("Liquid Swap", liquidSwap));
            break;

            default: break;
        }

        finalCleanupIndex = getStepIndex(tasks, "Final Cleanup");

        if (WorldConfig.Instance.EarlyHardMode)
        {
            tasks.Insert(finalCleanupIndex, new PassLegacy("Early Hardmode", startHardModeEarly));
            finalCleanupIndex = getStepIndex(tasks, "Final Cleanup");
        }

        if (WorldGenOptions.EvilSurface && WorldConfig.Instance.EvilToggle)
        {
            tasks.Insert(finalCleanupIndex, new PassLegacy("Toggle", ToggleRemixSeedOn));
            tasks.Insert(finalCleanupIndex + 2, new PassLegacy("Toggle", ToggleRemixSeedOff));

            finalCleanupIndex = getStepIndex(tasks, "Final Cleanup");
        }

            

        if (WorldConfig.Instance.HardModeOreMulti > 0)
        {
            tasks.Insert(finalCleanupIndex, new PassLegacy("Hardmode Ore", spawnHardModeOre));
            finalCleanupIndex = getStepIndex(tasks, "Final Cleanup");
        }

        //should be last final index one
        if (WorldGenOptions.PaintEverything)
        {
            tasks.Insert(finalCleanupIndex, new PassLegacy("Toggle", ToggleCelebrationSeedOn));
            tasks.Insert(finalCleanupIndex + 2, new PassLegacy("Toggle", ToggleCelebrationSeedOff));

            finalCleanupIndex = getStepIndex(tasks, "Final Cleanup");
        }

        int microIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Micro Biomes"));

        if (WorldGenOptions.MahoganyTrees)
        {
            tasks.Insert(microIndex, new PassLegacy("Toggle", ToggleDrunkSeedOn));
            tasks.Insert(microIndex + 2, new PassLegacy("Toggle", ToggleDrunkSeedOff));

            microIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Micro Biomes"));
        }

        multiplyStep(tasks, microIndex, WorldGenOptions.MicroMultiplier);

        int cleanupIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Tile Cleanup"));

        if (WorldGenOptions.BothEvils)
        {
            tasks.Insert(cleanupIndex, new PassLegacy("Toggle", ToggleDrunkSeedOn));
            tasks.Insert(cleanupIndex + 2, new PassLegacy("Toggle", ToggleDrunkSeedOff));
        }

        int treeIndex = getStepIndex(tasks, "Planting Trees");
        multiplyStep(tasks, treeIndex, WorldGenOptions.TreeMultiplier);

        int guideIndex = getStepIndex(tasks, "Guide");

        if (WorldConfig.Instance.startingNPC == WorldConfig.StartingNPC.PartyGroup)
        {
            tasks.Insert(guideIndex, new PassLegacy("Toggle", ToggleCelebrationSeedOn));
            tasks.Insert(guideIndex + 2, new PassLegacy("Toggle", ToggleCelebrationSeedOff));
        }
        else if (WorldConfig.Instance.startingNPC != WorldConfig.StartingNPC.Guide)
        {
            tasks.Insert(guideIndex + 1, new PassLegacy("Starting NPC", startingNPCStep));

            multiplyStep(tasks, guideIndex, 0);
        }

        int grassWallIndex = getStepIndex(tasks, "Grass Wall");

        if (WorldGenOptions.SurfaceSpiders)
        {
            tasks.Insert(grassWallIndex, new PassLegacy("Toggle", ToggleConstantSeedOn));
            tasks.Insert(grassWallIndex + 2, new PassLegacy("Toggle", ToggleConstantSeedOff));

            grassWallIndex = getStepIndex(tasks, "Grass Wall");
            multiplyStep(tasks, grassWallIndex, WorldGenOptions.SpiderMultiplier);
        }

        int spawnIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Spawn Point"));

        if (WorldConfig.Instance.spawnLocation == WorldConfig.SpawnLocation.Ocean)
        {
            tasks.Insert(spawnIndex, new PassLegacy("Toggle", ToggleCelebrationSeedOn));
            tasks.Insert(spawnIndex + 2, new PassLegacy("Toggle", ToggleCelebrationSeedOff));
            spawnIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Spawn Point"));
        }

        if (WorldConfig.Instance.spawnLocation == WorldConfig.SpawnLocation.Underworld && WorldConfig.Instance.UnderworldToggle)
        {
            tasks.Insert(spawnIndex, new PassLegacy("Toggle", ToggleRemixSeedOn));
            tasks.Insert(spawnIndex + 2, new PassLegacy("Toggle", ToggleRemixSeedOff));
            spawnIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Spawn Point"));
        }

        if (WorldConfig.Instance.spawnLocation == WorldConfig.SpawnLocation.Random)
        {
            tasks.Insert(spawnIndex + 1, new PassLegacy("Random Spawn", randomSpawn));

            spawnIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Spawn Point"));
        }

        int trapIndex = getStepIndex(tasks, "Traps");

        if (WorldGenOptions.NoTraps)
        {
            tasks.Insert(trapIndex, new PassLegacy("Toggle", ToggleTrapSeedOn));
            tasks.Insert(trapIndex + 2, new PassLegacy("Toggle", ToggleTrapSeedOff));

            trapIndex = getStepIndex(tasks, "Traps");
        }

        multiplyStep(tasks, trapIndex, WorldGenOptions.TrapMultiplier);

        int potIndex = getStepIndex(tasks, "Pots");
        multiplyStep(tasks, potIndex, WorldGenOptions.PotMultiplier);

        int jungleTreeIndex = getStepIndex(tasks, "Jungle Trees");
        multiplyStep(tasks, jungleTreeIndex, WorldGenOptions.TreeMultiplier);
            

        int spiderIndex = getStepIndex(tasks, "Spider Caves");

        //if (WorldConfig.Instance.MoreSpiders)
        //{
        //    tasks.Insert(spiderIndex, new PassLegacy("Toggle", ToggleWorthySeedOn));
        //    tasks.Insert(spiderIndex + 2, new PassLegacy("Toggle", ToggleWorthySeedOff));
        //    spiderIndex = getStepIndex(tasks, "Spider Caves");
        //}

        multiplyStep(tasks, spiderIndex, WorldGenOptions.SpiderMultiplier);

        //CHESTS
        int waterChestIndex = getStepIndex(tasks, "Water Chests");
        multiplyStep(tasks, waterChestIndex, WorldGenOptions.ChestMultiplier);
        int jungleChestIndex = getStepIndex(tasks, "Jungle Chests Placement");
        multiplyStep(tasks, jungleChestIndex, WorldGenOptions.ChestMultiplier);
        int surfaceChestIndex = getStepIndex(tasks, "Surface Chests");
        multiplyStep(tasks, surfaceChestIndex, WorldGenOptions.ChestMultiplier);
        int goldChestIndex = getStepIndex(tasks, "Buried Chests");

        if (WorldGenOptions.RainbowCabins)
        {
            tasks.Insert(goldChestIndex, new PassLegacy("Toggle", ToggleCelebrationSeedOn));
            tasks.Insert(goldChestIndex + 2, new PassLegacy("Toggle", ToggleCelebrationSeedOff));
            goldChestIndex = getStepIndex(tasks, "Buried Chests");
        }

        multiplyStep(tasks, goldChestIndex, WorldGenOptions.ChestMultiplier);

        if (WorldGenOptions.FishingShacks)
        {
            tasks.Insert(getStepIndex(tasks, "Hives") + 1, new PassLegacy("FishingShacks", NewFeatures.FishingShacks));
        }


        if (WorldGenOptions.Mineshafts)
        {
            tasks.Insert(getStepIndex(tasks, "Hives") + 1, new PassLegacy("Ravines", NewFeatures.Mineshafts));
        }

        if (WorldGenOptions.GrandCaverns)
        {
            tasks.Insert(getStepIndex(tasks, "Hives") + 1, new PassLegacy("GrandCaverns", NewFeatures.GrandCaverns));
        }

        if (WorldGenOptions.Graveyard)
        {
            tasks.Insert(getStepIndex(tasks, "Sunflowers") + 1, new PassLegacy("Graveyard", NewFeatures.Graveyard));
        }

        int statueIndex = getStepIndex(tasks, "Statues");
        multiplyStep(tasks, statueIndex, WorldGenOptions.StatueMultiplier);

        int lifeCrystalIndex = getStepIndex(tasks, "Life Crystals");
        multiplyStep(tasks, lifeCrystalIndex, WorldGenOptions.LifeCrystalMultiplier);

        int hiveIndex = getStepIndex(tasks, "Hives");

        if (WorldGenOptions.BigHives)
        {
            tasks.Insert(hiveIndex, new PassLegacy("Toggle", ToggleDrunkSeedOn));
            tasks.Insert(hiveIndex + 2, new PassLegacy("Toggle", ToggleDrunkSeedOff));
            hiveIndex = getStepIndex(tasks, "Hives");
        }

        multiplyStep(tasks, hiveIndex, WorldGenOptions.HiveMultiplier);

        int templeIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Jungle Temple"));
        multiplyStep(tasks, templeIndex, WorldConfig.Instance.JungleToggle ? 1 : 0); //no tmple if no jungle..

        if (WorldGenOptions.HugeTemple && WorldConfig.Instance.JungleToggle)
        {
            tasks.Insert(templeIndex, new PassLegacy("Toggle", ToggleWorthySeedOn));
            tasks.Insert(templeIndex + 2, new PassLegacy("Toggle", ToggleWorthySeedOff));
            templeIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Jungle Temple"));
        }

        if (WorldGenOptions.SurfaceTemple && WorldConfig.Instance.JungleToggle)
        {
            tasks.Insert(templeIndex, new PassLegacy("Toggle", ToggleRemixSeedOn));
            tasks.Insert(templeIndex + 2, new PassLegacy("Toggle", ToggleRemixSeedOff));
        }

        int livingTreeIndex = getStepIndex(tasks, "Living Trees");

        if (WorldGenOptions.LivingTrees)
        {
            tasks.Insert(livingTreeIndex, new PassLegacy("Toggle", ToggleDrunkSeedOn));
            tasks.Insert(livingTreeIndex + 2, new PassLegacy("Toggle", ToggleDrunkSeedOff));
            livingTreeIndex = getStepIndex(tasks, "Living Trees");
        }

        //multiplyStep(tasks, livingTreeIndex, WorldGenOptions.LivingTreeMultiplier);

        int pyramidIndex =  getStepIndex(tasks, "Pyramids");

        if (WorldGenOptions.PyramidEntrance)
        {
            tasks.Insert(pyramidIndex, new PassLegacy("Toggle", ToggleCelebrationSeedOn));
            tasks.Insert(pyramidIndex + 2, new PassLegacy("Toggle", ToggleCelebrationSeedOff));
        }

        int shimmerIndex = getStepIndex(tasks, "Shimmer");

        multiplyStep(tasks, shimmerIndex, WorldConfig.Instance.AetherToggle ? 1 : 0);

        int oceanCavesIndex = getStepIndex(tasks, "Create Ocean Caves");
        multiplyStep(tasks, oceanCavesIndex, WorldConfig.Instance.OceanToggle ? 1 : 0);

        if (WorldGenOptions.OceanCaves && WorldConfig.Instance.OceanToggle)
        {
            tasks.Insert(oceanCavesIndex, new PassLegacy("Toggle", ToggleDrunkSeedOn));
            tasks.Insert(oceanCavesIndex + 2, new PassLegacy("Toggle", ToggleDrunkSeedOff));
        }

        int gemIndex = getStepIndex(tasks, "Gems");
        multiplyStep(tasks, gemIndex, WorldGenOptions.GemMultiplier);

        int beachIndex = getStepIndex(tasks, "Beaches");
        multiplyStep(tasks, beachIndex, WorldConfig.Instance.OceanToggle ? 1 : 0);
        int shellIndex = getStepIndex(tasks, "Shell Piles"); //must also be skipped if ocean is

        if (WorldGenOptions.SurfaceMarble && WorldConfig.Instance.OceanToggle)
        {
            tasks.Insert(shellIndex, new PassLegacy("Toggle", ToggleConstantSeedOn));
            tasks.Insert(shellIndex + 2, new PassLegacy("Toggle", ToggleConstantSeedOff));

            shellIndex = getStepIndex(tasks, "Shell Piles");
            //multiplyStep(tasks, shellIndex, WorldConfig.Instance.MarbleMultiplier);
        }

        multiplyStep(tasks, shellIndex, WorldConfig.Instance.OceanToggle ? 1 : 0);

        int dungeonIndex = getStepIndex(tasks, "Dungeon");

        if (WorldGenOptions.UndergroundDungeon)
        {
            tasks.Insert(dungeonIndex, new PassLegacy("Toggle", ToggleDrunkSeedOn));
            tasks.Insert(dungeonIndex + 2, new PassLegacy("Toggle", ToggleDrunkSeedOff));
            dungeonIndex = getStepIndex(tasks, "Dungeon");
        }

        multiplyStep(tasks, dungeonIndex, WorldConfig.Instance.DungeonToggle ? 1 : 0);

        if (WorldConfig.Instance.SecondDungeon)
        {
            tasks.Insert(dungeonIndex + 1, new PassLegacy("2nd Dungeon", MakeSecondDungeon));
        }
            


        //int lakeIndex = getStepIndex(tasks, "Lakes");
        //multiplyStep(tasks, lakeIndex, WorldConfig.Instance.LakeMultiplier);

        int corruptionIndex = getStepIndex(tasks, "Corruption");
        multiplyStep(tasks, corruptionIndex, WorldConfig.Instance.EvilToggle ? 1 : 0);

        if (WorldGenOptions.BothEvils && WorldConfig.Instance.EvilToggle)
        {
            tasks.Insert(corruptionIndex, new PassLegacy("Toggle", ToggleDrunkSeedOn));
            tasks.Insert(corruptionIndex + 2, new PassLegacy("Toggle", ToggleDrunkSeedOff));
            corruptionIndex = getStepIndex(tasks, "Corruption");
        }

        int underworldIndex = getStepIndex(tasks, "Underworld");
        multiplyStep(tasks, underworldIndex, WorldConfig.Instance.UnderworldToggle ? 1 : 0);
        int hellForgeIndex = getStepIndex(tasks, "Hellforge"); //must also be skipped if underworld is
        multiplyStep(tasks, hellForgeIndex, WorldConfig.Instance.UnderworldToggle ? 1 : 0);

        if (WorldGenOptions.RemixUnderworld && WorldConfig.Instance.UnderworldToggle)
        {
            tasks.Insert(underworldIndex, new PassLegacy("Toggle", ToggleRemixSeedOn));
            tasks.Insert(underworldIndex + 2, new PassLegacy("Toggle", ToggleRemixSeedOff));
        }

        int shiniesIndex = getStepIndex(tasks, "Shinies");

        if (WorldGenOptions.BothOres)
        {
            tasks.Insert(shiniesIndex, new PassLegacy("Toggle", ToggleDrunkSeedOn));
            tasks.Insert(shiniesIndex + 2, new PassLegacy("Toggle", ToggleDrunkSeedOff));
            shiniesIndex = getStepIndex(tasks, "Shinies");
        }

        multiplyStep(tasks, shiniesIndex, WorldGenOptions.OreMultiplier);

        int marbleIndex = getStepIndex(tasks, "Marble");
        int graniteIndex = getStepIndex(tasks, "Granite");

        if (WorldGenOptions.MarbleGraniteSwapped)
        {
            tasks.Insert(marbleIndex, new PassLegacy("Toggle", ToggleDrunkSeedOn));
            tasks.Insert(graniteIndex + 2, new PassLegacy("Toggle", ToggleDrunkSeedOff));

            graniteIndex = getStepIndex(tasks, "Granite");
            marbleIndex = getStepIndex(tasks, "Marble");
        }

        multiplyStep(tasks, graniteIndex, WorldGenOptions.GraniteMultiplier);
        multiplyStep(tasks, marbleIndex, WorldGenOptions.MarbleMultiplier);

        int mushroomPatchIndex = getStepIndex(tasks, "Mushroom Patches");

        if (WorldGenOptions.MushroomLayer && WorldConfig.Instance.MushroomToggle)
        {
            tasks.Insert(mushroomPatchIndex, new PassLegacy("Toggle", ToggleRemixSeedOn));
            tasks.Insert(mushroomPatchIndex + 2, new PassLegacy("Toggle", ToggleRemixSeedOff));

            mushroomPatchIndex = getStepIndex(tasks, "Mushroom Patches");
        }

        multiplyStep(tasks, mushroomPatchIndex, WorldConfig.Instance.MushroomToggle ? 1 : 0);

        if (WorldGenOptions.SurfaceMushroom)
        {
            tasks.Insert(mushroomPatchIndex, new PassLegacy("Surface Mushroom", surfaceMushroom));
        }

        int floatingIslandHouseIndex = getStepIndex(tasks, "Floating Island Houses");
        int floatingIslandIndex = getStepIndex(tasks, "Floating Islands");

        if (WorldGenOptions.FloatingIslandMultiplier > 1)
        {
            GenPass floatingIslandHouseStep = tasks[floatingIslandHouseIndex];
            GenPass floatingIslandStep = tasks[floatingIslandIndex];

            for (int i = 1; i < WorldGenOptions.FloatingIslandMultiplier; i++)
            {
                tasks.Insert(floatingIslandIndex, floatingIslandHouseStep);
                tasks.Insert(floatingIslandIndex, floatingIslandStep);
            }
        }
        else
        {
            int multiplier = WorldGenOptions.FloatingIslandMultiplier;

            multiplyStep(tasks, floatingIslandIndex, multiplier);
            multiplyStep(tasks, floatingIslandHouseIndex, multiplier);
        }

        int desertIndex = getStepIndex(tasks, "Full Desert");
        multiplyStep(tasks, desertIndex, WorldConfig.Instance.DesertToggle ? 1 : 0);

        int jungleIndex = getStepIndex(tasks, "Jungle");
        multiplyStep(tasks, jungleIndex, WorldConfig.Instance.JungleToggle ? 1 : 0);
            

        int iceIndex = getStepIndex(tasks, "Generate Ice Biome");
        multiplyStep(tasks, iceIndex, WorldConfig.Instance.IceToggle ? 1 : 0);

        int wavyCaveIndex = getStepIndex(tasks, "Wavy Caves");

        if (WorldGenOptions.WavyCaves)
        {
            tasks.Insert(wavyCaveIndex, new PassLegacy("Toggle", ToggleConstantSeedOn));
            tasks.Insert(wavyCaveIndex + 2, new PassLegacy("Toggle", ToggleConstantSeedOff));
        }

        int surfaceCaveIndex = getStepIndex(tasks, "Surface Caves");

        if (WorldConfig.Instance.CavesToggle)
        {
            multiplyStep(tasks, surfaceCaveIndex, WorldGenOptions.CaveMultiplier);
        }
        else
        {
            multiplyStep(tasks, surfaceCaveIndex, 0);
        }

        int rockCaveIndex = getStepIndex(tasks, "Rock Layer Caves");
        multiplyStep(tasks, rockCaveIndex, WorldConfig.Instance.CavesToggle ? 1 : 0);

        int dirtCaveIndex = getStepIndex(tasks, "Dirt Layer Caves");
        multiplyStep(tasks, dirtCaveIndex, WorldConfig.Instance.CavesToggle ? 1 : 0);

        int mountainCaves = getStepIndex(tasks, "Mountain Caves");
        multiplyStep(tasks, mountainCaves, WorldConfig.Instance.CavesToggle ? 1 : 0);








































        //drunk seed
        //more living mahigany trees
        //desert floating island
        //snow floating island

        //not the bees
        //larva throughtout the world

        //for the worthy
        //more lava pools
        //more glowing mushroom
        //evil floating island
        //spiky dungeon
        //big glowing moss biome
        //underworld houses are hellstone
        //more lava underworld
        //some chests have a chance to have an angel statue in the first slot instead of a good thing

        //celebration
        //hallow floating island


        //dont dig up
        //scary surface
        //surface temple
        //dead living trees (lol
        //glowing mush layer
        //underworld flat

        //get fixed boi
        //water becomes honey in layer


        /*ideas
         *

        spawn set in various biomes
        //seeds active after world gen*/






        base.ModifyWorldGenTasks(tasks, ref totalWeight);
    }

    private void ToggleDrunkSeedOn(GenerationProgress progress, GameConfiguration configuration)
    {
        WorldGen.drunkWorldGen = true;
    }

    private void ToggleDrunkSeedOff(GenerationProgress progress, GameConfiguration configuration)
    {
        if (!drunkSeed)
        {
            WorldGen.drunkWorldGen = false;
        }
    }

    private void ToggleCelebrationSeedOn(GenerationProgress progress, GameConfiguration configuration)
    {
        Main.tenthAnniversaryWorld = true;
        WorldGen.tenthAnniversaryWorldGen = true;
    }

    private void ToggleCelebrationSeedOff(GenerationProgress progress, GameConfiguration configuration)
    {
        if (!celebrationSeed)
        {
            Main.tenthAnniversaryWorld = false;
            WorldGen.tenthAnniversaryWorldGen = false;
        }
    }

    private void ToggleWorthySeedOn(GenerationProgress progress, GameConfiguration configuration)
    {
        WorldGen.getGoodWorldGen = true;
    }

    private void ToggleWorthySeedOff(GenerationProgress progress, GameConfiguration configuration)
    {
        if (!ftwSeed)
        {
            WorldGen.getGoodWorldGen = false;
        }
    }

    private void ToggleConstantSeedOn(GenerationProgress progress, GameConfiguration configuration)
    {
        WorldGen.dontStarveWorldGen = true;
    }

    private void ToggleConstantSeedOff(GenerationProgress progress, GameConfiguration configuration)
    {
        if (!starveSeed)
        {
            WorldGen.dontStarveWorldGen = false;
        }
    }

    private void ToggleTrapSeedOn(GenerationProgress progress, GameConfiguration configuration)
    {
        WorldGen.noTrapsWorldGen = true;
    }

    private void ToggleTrapSeedOff(GenerationProgress progress, GameConfiguration configuration)
    {
        if (!trapSeed)
        {
            WorldGen.noTrapsWorldGen = false;
        }
    }

    private void ToggleRemixSeedOn(GenerationProgress progress, GameConfiguration configuration)
    {
        WorldGen.remixWorldGen = true;
    }

    private void ToggleRemixSeedOff(GenerationProgress progress, GameConfiguration configuration)
    {
        if (!remixSeed)
        {
            WorldGen.remixWorldGen = false;
        }
    }

    private void MakeSecondDungeon(GenerationProgress progress, GameConfiguration configuration)
    {
        int dungeonSide = GenVars.dungeonSide;
        int newDungeonLocation;

        if (dungeonSide == -1)
        {
            newDungeonLocation = GenVars.dungeonLocation = WorldGen.genRand.Next((int)((double)Main.maxTilesX * 0.8), GenVars.rightBeachStart - 50);
        }
        else
        {
            newDungeonLocation = WorldGen.genRand.Next(GenVars.leftBeachEnd + 50, (int)((double)Main.maxTilesX * 0.2));
        }

        int num755 = (int)((Main.worldSurface + Main.rockLayer) / 2.0) + WorldGen.genRand.Next(-200, 200);
        int num756 = (int)((Main.worldSurface + Main.rockLayer) / 2.0) + 200;
        int num757 = num755;

        bool flag47 = false;
        for (int num758 = 0; num758 < 10; num758++)
        {
            if (WorldGen.SolidTile(newDungeonLocation, num757 + num758))
            {
                flag47 = true;
                break;
            }
        }
        if (!flag47)
        {
            for (; num757 < num756 && !WorldGen.SolidTile(newDungeonLocation, num757 + 10); num757++)
            {
            }
        }

        WorldGen.MakeDungeon(newDungeonLocation, num757);
        //Console.WriteLine("dungeon side: " + GenVars.dungeonLocation);
        //Console.WriteLine("max x: " + Main.maxTilesX);
    }

    private int getStepIndex(List<GenPass> tasks, string name)
    {
        return tasks.FindIndex(genPass => genPass.Name.Equals(name));
    }

    private void multiplyStep(List<GenPass> tasks, int stepIndex, int numMultiplier)
    {
        GenPass pass = tasks[stepIndex];
        string ogName = pass.Name;

        if (numMultiplier == 0)
        {
            tasks.Remove(pass);
        }

        for (int i = 1; i < numMultiplier; i++)
        {
            tasks.Insert(stepIndex, pass);

            //Console.WriteLine(ogName + " " + (i + 1));
        }
    }

    private void replaceLiquidWithLava(GenerationProgress progress, GameConfiguration configuration)
    {
        replaceAllLiquid(LiquidID.Lava);
    }

    private void replaceLiquidWithWater(GenerationProgress progress, GameConfiguration configuration)
    {
        replaceAllLiquid(LiquidID.Water);
    }

    private void replaceLiquidWithHoney(GenerationProgress progress, GameConfiguration configuration)
    {
        replaceAllLiquid(LiquidID.Honey);
    }

    private void replaceLiquidWithShimmer(GenerationProgress progress, GameConfiguration configuration)
    {
        replaceAllLiquid(LiquidID.Shimmer);
    }

    private void replaceAllLiquid(short liquidType)
    {
        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY; j++)
            {
                if (Main.tile[i, j].LiquidAmount > 0)
                {
                    Tile tile = Main.tile[i, j];
                    tile.LiquidType = liquidType;
                }
            }
        }
    }

    private void liquidSwap(GenerationProgress progress, GameConfiguration configuration)
    {
        List<short> liquidTypes = new List<short>();
        liquidTypes.Add(LiquidID.Lava);
        liquidTypes.Add(LiquidID.Honey);
        liquidTypes.Add(LiquidID.Shimmer);

        short waterReplacement = Main.rand.NextFromCollection(liquidTypes);

        liquidTypes = new List<short>();
        liquidTypes.Add(LiquidID.Water);
        liquidTypes.Add(LiquidID.Honey);
        liquidTypes.Add(LiquidID.Shimmer);
        liquidTypes.Remove(waterReplacement);

        short lavaReplacement = Main.rand.NextFromCollection(liquidTypes);

        liquidTypes = new List<short>();
        liquidTypes.Add(LiquidID.Water);
        liquidTypes.Add(LiquidID.Lava);
        liquidTypes.Add(LiquidID.Shimmer);
        liquidTypes.Remove(waterReplacement);
        liquidTypes.Remove(lavaReplacement);

        short honeyReplacement = Main.rand.NextFromCollection(liquidTypes);

        //shimmer cant pick itself so if it is still there, honey takes it
        if (liquidTypes.Contains(LiquidID.Shimmer))
        {
            honeyReplacement = LiquidID.Shimmer;
        }

        liquidTypes = new List<short>();
        liquidTypes.Add(LiquidID.Water);
        liquidTypes.Add(LiquidID.Lava);
        liquidTypes.Add(LiquidID.Honey);
        liquidTypes.Remove(waterReplacement);
        liquidTypes.Remove(lavaReplacement);
        liquidTypes.Remove(honeyReplacement);

        short shimmerReplacement = liquidTypes[0];

        //if (liquidTypes.Count > 0)
        //{
        //    shimmerReplacement = 
        //}

            

        //Console.WriteLine(waterReplacement + " " + lavaReplacement + " " + honeyReplacement + " " + shimmerReplacement);

        //get tiles with liquid type
        List<Tile> waterTileList = getTileListWithLiquidType(LiquidID.Water);
        List<Tile> lavaTileList = getTileListWithLiquidType(LiquidID.Lava);
        List<Tile> honeyTileList = getTileListWithLiquidType(LiquidID.Honey);
        List<Tile> shimmerTileList = getTileListWithLiquidType(LiquidID.Shimmer);

        replaceTileListWithLiquidType(waterTileList, waterReplacement);
        replaceTileListWithLiquidType(lavaTileList, lavaReplacement);
        replaceTileListWithLiquidType(honeyTileList, honeyReplacement);
        replaceTileListWithLiquidType(shimmerTileList, shimmerReplacement);

    }

    private List<Tile> getTileListWithLiquidType(short type)
    {
        List<Tile> tileList = new List<Tile>();

        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY; j++)
            {
                if (Main.tile[i, j].LiquidAmount > 0 && Main.tile[i, j].LiquidType == type)
                {
                    Tile tile = Main.tile[i, j];
                    tileList.Add(tile);
                }
            }
        }

        return tileList;
    }

    private void replaceTileListWithLiquidType(List<Tile> tileList, short type)
    {
        for (int i = 0; i < tileList.Count; i++)
        {
            Tile tile = tileList[i];
            tile.LiquidType = type;
        }
    }

    private void replaceAllLiquidOfType(short liquidType, short typeToReplace)
    {
        for (int i = 0; i < Main.maxTilesX; i++)
        {
            for (int j = 0; j < Main.maxTilesY; j++)
            {
                if (Main.tile[i, j].LiquidAmount > 0)
                {
                    Tile tile = Main.tile[i, j];
                    tile.LiquidType = liquidType;
                }
            }
        }
    }

    private void startHardModeEarly(GenerationProgress progress, GameConfiguration configuration)
    {
        WorldGen.StartHardmode();
    }

    private void surfaceMushroom(GenerationProgress progress, GameConfiguration configuration)
    {
        if (WorldGenOptions.SurfaceMushroom)
        {
            int shroomX = Main.rand.Next(100, Main.maxTilesX - 100);
            int shroomY = (int)Main.worldSurface + Main.rand.Next(-120, -80);
            for (int attempt = 0; attempt < 50000; attempt++)
            {
                if (!Main.tile[shroomX, shroomY].HasTile || Main.tile[shroomX, shroomY].TileType != TileID.Mud)
                {
                    shroomX = Main.rand.Next(100, Main.maxTilesX - 100);
                    shroomY = (int)Main.worldSurface + Main.rand.Next(-120, -80);
                }
            }

            shroomY += 60;
                

            WorldGen.ShroomPatch(shroomX, shroomY);

            for (int num939 = 0; num939 < 5; num939++)
            {
                int i4 = shroomX + WorldGen.genRand.Next(-40, 41);
                int j6 = shroomY + WorldGen.genRand.Next(-40, 41);
                WorldGen.ShroomPatch(i4, j6);
            }

            //spread mushroom grass on all mud
            for (int num940 = 0; num940 < Main.maxTilesX; num940++)
            {
                for (int num941 = 0; num941 < Main.worldSurface; num941++)
                {
                    if (WorldGen.InWorld(num940, num941, 50) && Main.tile[num940, num941].HasTile)
                    {
                        WorldGen.grassSpread = 0;
                        WorldGen.SpreadGrass(num940, num941, 59, 70, repeat: false);
                    }
                }
            }
            //add random holes
            for (int num942 = 0; num942 < Main.maxTilesX; num942++)
            {
                for (int num943 = 0; num943 < (int)Main.worldSurface; num943++)
                {
                    if (Main.tile[num942, num943].HasTile && Main.tile[num942, num943].TileType == 70)
                    {
                        int type10 = 59;
                        for (int num944 = num942 - 1; num944 <= num942 + 1; num944++)
                        {
                            for (int num945 = num943 - 1; num945 <= num943 + 1; num945++)
                            {
                                if (Main.tile[num944, num945].HasTile)
                                {
                                    if (!Main.tile[num944 - 1, num945].HasTile && !Main.tile[num944 + 1, num945].HasTile)
                                    {
                                        WorldGen.KillTile(num944, num945);
                                    }
                                    else if (!Main.tile[num944, num945 - 1].HasTile && !Main.tile[num944, num945 + 1].HasTile)
                                    {
                                        WorldGen.KillTile(num944, num945);
                                    }
                                }
                                else if (Main.tile[num944 - 1, num945].HasTile && Main.tile[num944 + 1, num945].HasTile)
                                {
                                    WorldGen.PlaceTile(num944, num945, type10);
                                    if (Main.tile[num944 - 1, num943].TileType == 70)
                                    {
                                        Main.tile[num944 - 1, num943].TileType = 59;
                                    }
                                    if (Main.tile[num944 + 1, num943].TileType == 70)
                                    {
                                        Main.tile[num944 + 1, num943].TileType = 59;
                                    }
                                }
                                else if (Main.tile[num944, num945 - 1].HasTile && Main.tile[num944, num945 + 1].HasTile)
                                {
                                    WorldGen.PlaceTile(num944, num945, type10);
                                    if (Main.tile[num944, num943 - 1].TileType == 70)
                                    {
                                        Main.tile[num944, num943 - 1].TileType = 59;
                                    }
                                    if (Main.tile[num944, num943 + 1].TileType == 70)
                                    {
                                        Main.tile[num944, num943 + 1].TileType = 59;
                                    }
                                }
                            }
                        }
                        if (WorldGen.genRand.Next(4) == 0)
                        {
                            int num946 = num942 + WorldGen.genRand.Next(-20, 21);
                            int num947 = num943 + WorldGen.genRand.Next(-20, 21);
                            if (WorldGen.InWorld(num946, num947) && Main.tile[num946, num947].TileType == 59)
                            {
                                Main.tile[num946, num947].TileType = 70;
                            }
                        }
                    }
                }
            }
        }
    }

    private void secondShimmer(GenerationProgress progress, GameConfiguration configuration)
    {
        int num702 = 50;
        int num703 = (int)(Main.worldSurface + Main.rockLayer) / 2 + num702;
        int num704 = (int)((double)((Main.maxTilesY - 250) * 2) + Main.rockLayer) / 3;
        if (num704 > Main.maxTilesY - 330 - 100 - 30)
        {
            num704 = Main.maxTilesY - 330 - 100 - 30;
        }
        if (num704 <= num703)
        {
            num704 = num703 + 50;
        }
        int num705 = WorldGen.genRand.Next(num703, num704);
        int num706 = ((GenVars.dungeonSide > 0) ? WorldGen.genRand.Next((int)((double)Main.maxTilesX * 0.89), Main.maxTilesX - 200) : WorldGen.genRand.Next(200, (int)((double)Main.maxTilesX * 0.11)));
        int num707 = (int)Main.worldSurface + 150;
        int num708 = (int)(Main.rockLayer + Main.worldSurface + 200.0) / 2;
        if (num708 <= num707)
        {
            num708 = num707 + 50;
        }
        if (WorldGen.tenthAnniversaryWorldGen)
        {
            num705 = WorldGen.genRand.Next(num707, num708);
        }
        int num709 = 0;

        //int shimmerX = Main.rand.Next(100, Main.maxTilesX - 100);
        //int shimmerY = (int)Main.worldSurface + Main.rand.Next(-400, -300);

        while (!WorldGen.ShimmerMakeBiome(num706, num705))
        {
            num709++;
            if (WorldGen.tenthAnniversaryWorldGen && num709 < 10000)
            {
                num705 = WorldGen.genRand.Next(num707, num708);
                num706 = ((GenVars.dungeonSide < 0) ? WorldGen.genRand.Next((int)((double)Main.maxTilesX * 0.89), Main.maxTilesX - 200) : WorldGen.genRand.Next(200, (int)((double)Main.maxTilesX * 0.11)));
            }
            else if (num709 > 20000)
            {
                num705 = WorldGen.genRand.Next((int)Main.worldSurface + 100 + 20, num704);
                num706 = ((GenVars.dungeonSide < 0) ? WorldGen.genRand.Next((int)((double)Main.maxTilesX * 0.8), Main.maxTilesX - 200) : WorldGen.genRand.Next(200, (int)((double)Main.maxTilesX * 0.2)));
            }
            else
            {
                num705 = WorldGen.genRand.Next((int)(Main.worldSurface + Main.rockLayer) / 2 + 20, num704);
                num706 = ((GenVars.dungeonSide < 0) ? WorldGen.genRand.Next((int)((double)Main.maxTilesX * 0.89), Main.maxTilesX - 200) : WorldGen.genRand.Next(200, (int)((double)Main.maxTilesX * 0.11)));
            }
        }



    }

    private void spawnHardModeOre(GenerationProgress progress, GameConfiguration configuration)
    {
        for (int multi = 0; multi < WorldConfig.Instance.HardModeOreMulti; multi++)
        {
            //for each ore tier
            for (int i = 0; i < 3; i++)
            {
                //int num = i;
                int num2 = 1;
                double num3 = (double)Main.maxTilesX / 4200.0;
                int num4 = 1 - i;
                num3 = num3 * 310.0 - (double)(85 * i);
                num3 *= 0.85;
                num3 /= (double)num2;
                //bool flag = false;

                int oreToSpawn;

                //for each alt ore
                for (int j = 0; j < 2; j++)
                {
                    switch (i)
                    {
                        case 0:
                        {
                            int num6 = 12;
                            if (j == 1)
                            {
                                num6 += 9;
                                num3 *= 0.89999997615814209;
                            }

                            oreToSpawn = (j == 0) ? TileID.Cobalt : TileID.Palladium;
                            num3 *= 1.0499999523162842;
                            break;
                        }
                        case 1:
                        {
                            int num7 = 13;
                            if (j == 1)
                            {
                                num7 += 9;
                                num3 *= 0.89999997615814209;
                            }

                            oreToSpawn = (j == 0) ? TileID.Mythril : TileID.Orichalcum;
                            break;
                        }
                        default:
                        {
                            int num5 = 14;
                            if (j == 1)
                            {
                                num5 += 9;
                                num3 *= 0.89999997615814209;
                            }

                            oreToSpawn = (j == 0) ? TileID.Adamantite : TileID.Titanium;

                            break;
                        }
                    }

                    for (int k = 0; (double)k < num3; k++)
                    {
                        int i2 = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                        double num8 = Main.worldSurface;
                        if (i == 1)
                        {
                            num8 = Main.rockLayer;
                        }
                        if (i == 2)
                        {
                            num8 = (Main.rockLayer + Main.rockLayer + (double)Main.maxTilesY) / 3.0;
                        }
                        int j2 = WorldGen.genRand.Next((int)num8, Main.maxTilesY - 150);
                        if (Main.remixWorld)
                        {
                            double num9 = Main.maxTilesX - 350;
                            if (i == 1)
                            {
                                num9 = (Main.rockLayer + Main.rockLayer + (double)Main.maxTilesY - 350.0) / 3.0;
                            }
                            if (i == 2)
                            {
                                num9 = Main.rockLayer - 25.0;
                            }
                            j2 = WorldGen.genRand.Next((int)Main.worldSurface + 15, (int)num9);
                        }

                        Console.WriteLine(oreToSpawn);

                        WorldGen.OreRunner(i2, j2, WorldGen.genRand.Next(5, 9 + num4), WorldGen.genRand.Next(5, 9 + num4), (ushort)oreToSpawn);
                    }
                }
            }
        }
    }

    private void randomSpawn(GenerationProgress progress, GameConfiguration configuration)
    {
        List<int> unsafeWallList = new List<int>();
        unsafeWallList.Add(WallID.BlueDungeon);
        unsafeWallList.Add(WallID.BlueDungeonSlab);
        unsafeWallList.Add(WallID.BlueDungeonSlabUnsafe);
        unsafeWallList.Add(WallID.BlueDungeonTile);
        unsafeWallList.Add(WallID.BlueDungeonTileUnsafe);
        unsafeWallList.Add(WallID.BlueDungeonUnsafe);

        unsafeWallList.Add(WallID.GreenDungeon); 
        unsafeWallList.Add(WallID.GreenDungeonSlab);
        unsafeWallList.Add(WallID.GreenDungeonSlabUnsafe);
        unsafeWallList.Add(WallID.GreenDungeonTile);
        unsafeWallList.Add(WallID.GreenDungeonTileUnsafe);
        unsafeWallList.Add(WallID.GreenDungeonUnsafe);

        unsafeWallList.Add(WallID.PinkDungeon);
        unsafeWallList.Add(WallID.PinkDungeonSlab);
        unsafeWallList.Add(WallID.PinkDungeonSlabUnsafe);
        unsafeWallList.Add(WallID.PinkDungeonTile);
        unsafeWallList.Add(WallID.PinkDungeonTileUnsafe);
        unsafeWallList.Add(WallID.PinkDungeonUnsafe);

        unsafeWallList.Add(WallID.LihzahrdBrick);
        unsafeWallList.Add(WallID.LihzahrdBrickUnsafe);
           



        bool lookingForSpawn = true;
        int spawnX;
        int spawnY;

        while (lookingForSpawn)
        {
            spawnX = Main.rand.Next(100, Main.maxTilesX);
            spawnY = Main.rand.Next(100, Main.maxTilesY);

            if (Main.tile[spawnX, spawnY].HasTile)
            {
                Main.spawnTileX = spawnX;
                Main.spawnTileY = spawnY;
                lookingForSpawn = false;
            }

            //want to make sure at least 3 tiles above this tile are open
            if (Main.tile[spawnX, spawnY - 1].HasTile || Main.tile[spawnX, spawnY - 2].HasTile || Main.tile[spawnX, spawnY - 3].HasTile)
            {
                lookingForSpawn = true;
            }




            if (Main.tile[Main.spawnTileX, Main.spawnTileY - 1].LiquidAmount > 0)
            {
                lookingForSpawn = true;
            }

            if (unsafeWallList.Contains(Main.tile[Main.spawnTileX, Main.spawnTileY].WallType))
            {
                lookingForSpawn = true;
            }
        }
    }

    private void startingNPCStep(GenerationProgress progress, GameConfiguration configuration)
    {
        switch (WorldConfig.Instance.startingNPC)
        {
            case WorldConfig.StartingNPC.Merchant:
                spawnStartingNPC(NPCID.Merchant);
                NPC.unlockedMerchantSpawn = true;
            break;
            case WorldConfig.StartingNPC.Demolitionist:
                spawnStartingNPC(NPCID.Demolitionist);
                NPC.unlockedDemolitionistSpawn = true;
            break;
            case WorldConfig.StartingNPC.PartyGirl:
                spawnStartingNPC(NPCID.PartyGirl);
                NPC.unlockedPartyGirlSpawn = true;
            break;
            case WorldConfig.StartingNPC.TaxCollector:
                spawnStartingNPC(NPCID.TaxCollector);
                NPC.savedTaxCollector = true;
            break;
            case WorldConfig.StartingNPC.Angler:
                spawnStartingNPC(NPCID.Angler);
                NPC.savedAngler = true;
            break;
        }
    }

    private void spawnStartingNPC(int npcType)
    {
        int num294 = NPC.NewNPC(new EntitySource_WorldGen(), Main.spawnTileX * 16, Main.spawnTileY * 16, npcType);
        Main.npc[num294].homeTileX = Main.spawnTileX;
        Main.npc[num294].homeTileY = Main.spawnTileY;
        Main.npc[num294].direction = 1;
        Main.npc[num294].homeless = true;
    }

    public override void PostWorldGen()
    {
        //if (WorldConfig.Instance.NoEvilSpread)
        //{
        //    WorldGen.AllowedToSpreadInfections = false;
        //}

            
    }

    public override void PreUpdateWorld()
    {
            
            
    }


}