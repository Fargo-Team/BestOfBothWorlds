using Iced.Intel;
using Microsoft.Xna.Framework;
using StructureHelper;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static Terraria.GameContent.Animations.Actions.Sprites;

namespace FargoSeeds.WorldGeneration;

public static class NewFeatures
{
    public static Point FindGround(Point p, bool allowPlatforms = false)
    {
        bool solid(Point po)
        {
            Tile tile = Main.tile[po];
            return WorldGen.SolidTile(po) || allowPlatforms && tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]);
        }
        if (p.X > 0 && p.Y > 0 && WorldGen.InWorld(p.X, p.Y, 2))
        {
            Point result = p;
            if (solid(result))
            {
                while (solid(new(result.X, result.Y - 1)) && result.Y >= 1)
                {
                    result.Y--;
                }
            }
            else
            {
                while (!solid(new(result.X, result.Y + 1)) && result.Y < Main.maxTilesY)
                {
                    result.Y++;
                }
            }
            if (result.X > 0 && result.Y > 0 && WorldGen.InWorld(result.X, result.Y, 2))
                return result;
        }
        return p;
    }

    private static readonly bool[] BlacklistedTiles = TileID.Sets.Factory.CreateBoolSet(true, 225, 41, 43, 44, 226, 203, 112, 25, 151, 21, 467);

    public static Point16 FishingShackSize;
    enum FishingShackTypes
    {
        Base,
        Desert,
        Jungle,
        Mushroom,
        Tundra,
        None
    }
    private static FishingShackTypes GetFishingShackType(Point origin, int sizeX, int sizeY)
    {
        int sizeTotal = sizeX * sizeY;
        float sizeRequirement = sizeTotal * 0.6f;

        int tile = Main.tile[origin].TileType;
        if (tile == TileID.Stone || tile == TileID.Dirt)
        {
            // check for normal biome
            Dictionary<ushort, int> tileDictionary = [];
            WorldUtils.Gen(origin, new Shapes.Rectangle(sizeX, sizeY), new Actions.TileScanner(TileID.Stone, TileID.Dirt).Output(tileDictionary));
            if (tileDictionary[TileID.Stone] + tileDictionary[TileID.Dirt] > sizeRequirement)
                return FishingShackTypes.Base;
            return FishingShackTypes.None;
        }
        if (tile == TileID.JungleGrass || tile == TileID.Mud) // jungle or mushroom biome
        {
            // check for jungle
            Dictionary<ushort, int> tileDictionary = [];
            WorldUtils.Gen(origin, new Shapes.Rectangle(sizeX, sizeY), new Actions.TileScanner(TileID.Mud, TileID.JungleGrass).Output(tileDictionary));
            if (tileDictionary[TileID.Mud] + tileDictionary[TileID.JungleGrass] > sizeRequirement && tileDictionary[TileID.JungleGrass] > 10)
                return FishingShackTypes.Jungle;

            /*
            // check for mushroom biome
            tileDictionary = [];
            WorldUtils.Gen(origin, new Shapes.Rectangle(sizeX, sizeY), new Actions.TileScanner(TileID.Mud, TileID.MushroomGrass).Output(tileDictionary));
            if (tileDictionary[TileID.Mud] + tileDictionary[TileID.MushroomGrass] > sizeRequirement)
                return FishingShackTypes.Mushroom;
            */
            return FishingShackTypes.None;
        }
        if (tile == TileID.IceBlock || tile == TileID.SnowBlock)
        {

            // check for tundra biome
            Dictionary<ushort, int> tileDictionary = [];
            WorldUtils.Gen(origin, new Shapes.Rectangle(sizeX, sizeY), new Actions.TileScanner(TileID.SnowBlock, TileID.IceBlock).Output(tileDictionary));
            if (tileDictionary[TileID.SnowBlock] + tileDictionary[TileID.IceBlock] > sizeRequirement)
                return FishingShackTypes.Tundra;

            return FishingShackTypes.None;
        }
        return FishingShackTypes.None;
    }
    public static bool TryPlaceFishingShack(Point origin)
    {

        int extraX = WorldGen.genRand.Next(36, 42);
        int extraY = WorldGen.genRand.Next(7, 12);
        int extraYUp = (int)(extraY * 0.65f);
        int xPadding = 0;
        int totalWidth = FishingShackSize.X + extraX + xPadding;
        int totalHeight = FishingShackSize.Y + extraY + extraYUp;
        int dir = WorldGen.genRand.NextBool() ? 1 : -1;

        Point16 shackPos = new(origin.X, origin.Y);
        if (!WorldGen.InWorld(origin.X, origin.Y, 10))
            return false;

        var type = GetFishingShackType(origin, totalWidth, (int)(totalHeight * 1.5f));
        if (type == FishingShackTypes.None)
            return false;

        // turn origin into top left of expanded structure area
        if (dir < 0)
            origin.X -= extraX;
        else
            origin.X -= xPadding;
        origin.Y -= extraY;
        Rectangle rect = new(origin.X, origin.Y, FishingShackSize.X + extraX + xPadding, FishingShackSize.Y + extraY + extraYUp);

        if (WorldUtils.Find(new Point(rect.X - 2, rect.Y - 2), Searches.Chain(new Searches.Rectangle(rect.Width + 4, rect.Height + 4).RequireAll(mode: false), new Conditions.HasLava()), out var _))
            return false;

        if (GenVars.structures != null && !GenVars.structures.CanPlace(rect, BlacklistedTiles, 10))
            return false;

        string extra = "_" + type.ToString();

        // dig out opening
        Point digoutPos = new(origin.X, origin.Y + (int)(extraYUp * 1f));
        float passes = 6;
        for (int i = 0; i < passes; i++)
        {
            if (i == 0 && dir > 0 || i == passes - 1 && dir < 0)
                continue;
            digoutPos.X += (int)(totalWidth / passes);
            WorldGen.TileRunner(digoutPos.X, digoutPos.Y, totalWidth * 0.55f, 10, -1, speedX: WorldGen.genRand.NextBool() ? 4f : -4f);
        }
        ushort tileType = type switch
        {
            FishingShackTypes.Tundra => TileID.IceBlock,
            FishingShackTypes.Jungle => TileID.Mud,
            FishingShackTypes.Desert => TileID.Sandstone,
            FishingShackTypes.Mushroom => TileID.Mud,
            _ => TileID.Stone
        };
        // make island
        var slime = new Shapes.Slime((int)(totalWidth * 0.7f), 1f, 0.4f);
        ShapeData shape = new ShapeData();
        int islandX = origin.X + dir * (int)(extraX * 0f);
        if (dir > 0)
        {
            islandX += totalWidth / 2;
        }
        else
        {
            islandX += totalWidth / 2;
        }
        Point islandPos = new(islandX, origin.Y + totalHeight);
        // make the island blob
        WorldUtils.Gen(islandPos, slime, Actions.Chain(new Modifiers.Blotches(2, 1, 0.8), new Actions.SetTile(tileType), new Actions.SetFrames(frameNeighbors: true).Output(shape)));
        // cut out blocks above house ground level
        Point cutoutPos = new(origin.X - 6, origin.Y + extraY - 1);
        var cutoutRect = new Shapes.Rectangle(new(0, 0, totalWidth + 12, FishingShackSize.Y));
        WorldUtils.Gen(cutoutPos, cutoutRect, Actions.Chain(new Modifiers.Blotches(2, 0.4), new Actions.ClearTile(frameNeighbors: true).Output(shape)));
        // cut out lake
        Point lakePos = islandPos;
        lakePos.Y -= (int)(extraY * 0.9f);
        lakePos.X += dir * (int)(extraX * 0.27f);
        var lake = new Shapes.Slime((int)(totalWidth * 0.35f), 1f, 1.2f);
        WorldUtils.Gen(lakePos, lake, Actions.Chain(new Modifiers.Blotches(2, 1, 0.8), new Actions.ClearTile(frameNeighbors: true).Output(shape)));
        // make water
        WorldUtils.Gen(lakePos, lake, Actions.Chain(new Modifiers.RectangleMask(-extraX - 4, extraX + 4, 3, extraY + 3), new Modifiers.IsEmpty(), new Actions.SetLiquid()));

        if (type == FishingShackTypes.Jungle) // grassify
        {
            for (int i = origin.X - 20; i < origin.X + totalWidth + 40; i++)
            {
                for (int j = origin.Y - 20; j < origin.Y + totalHeight + 40; j++)
                {
                    WorldGen.SpreadGrass(i, j, TileID.Mud, TileID.JungleGrass);
                }
            }
                
        }

        // fill in frame
        for (int i = 0; i < FishingShackSize.X; i++)
        {
            for (int j = 0; j < FishingShackSize.Y; j++)
            {
                Point p = new(shackPos.X + i, shackPos.Y + j);
                Tile t = Main.tile[p];
                if (t.TileType == TileID.ItemFrame)
                {
                    int num13 = TEItemFrame.Find(p.X, p.Y);
                    if (num13 != -1 && ((TEItemFrame)TileEntity.ByID[num13]).item.stack > 0)
                    {
                            
                    }
                }
            }
        }

        string path = "WorldGeneration/FishingShack";
        Mod mod = FargoSeeds.Mod;
        StructureHelper.API.Generator.GenerateStructure(path + extra, shackPos, mod);
        GenVars.structures?.AddProtectedStructure(rect, 20);
        return true;
    }
    public static void FishingShacks(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.FargoSeeds.WorldGenMessages.FishingShacks");

        string path = "WorldGeneration/FishingShack";
        Mod mod = FargoSeeds.Mod;
        if (FishingShackSize == Point16.Zero)
            FishingShackSize = StructureHelper.API.Generator.GetStructureDimensions(path + "_Base", mod);

        int amt = 8 + WorldGen.GetWorldSize() * 4;

        for (int i = 0; i < amt; i++)
        {
            progress.Set((float)i / amt);

            int attempts = 15000;

            int xBase = Main.maxTilesX * i / amt;
            int xVar = (int)(Main.maxTilesX / amt);

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                int x = xBase + WorldGen.genRand.Next(-xVar, xVar);
                if (x < 200 || x > Main.maxTilesX - 200)
                    continue;
                int y = WorldGen.genRand.Next((int)(GenVars.worldSurfaceHigh + 230), Main.maxTilesY - 230);

                Point origin = new(x, y);
                if (TryPlaceFishingShack(origin))
                    break;
            }
        }
    }
    public static bool TryPlaceRopeShaft(Point origin)
    {
        int topSectionHeight = 8;
        int sizeX = WorldGen.genRand.Next(16, 22);
        int sizeY = WorldGen.genRand.Next(80, 140);
        int sizeTotal = sizeX * sizeY;
        float sizeRequirement = sizeTotal * 0.25f;

        int tile = Main.tile[origin].TileType;

        if (!WorldGen.InWorld(origin.X, origin.Y, topSectionHeight + 4))
            return false;

        if (!WorldGen.InWorld(origin.X, origin.Y + sizeY, 4))
            return false;

        // offset to top section
        origin.Y -= topSectionHeight;
        ushort[] checkTypes = [];
        ushort tileType = 0;
        ushort platformVariant = 0;
            
        if (tile == TileID.Stone || tile == TileID.Dirt)
        {
            tileType = TileID.Stone;
            checkTypes = [TileID.Stone, TileID.Dirt];
        }
        else if (tile == TileID.IceBlock || tile == TileID.SnowBlock)
        {
            tileType = TileID.IceBlock;
            checkTypes = [TileID.IceBlock, TileID.SnowBlock];
            platformVariant = 19; // boreal
        }
        else if (tile == TileID.JungleGrass)
        {
            tileType = TileID.Mud;
            checkTypes = [TileID.Mud, TileID.JungleGrass];
            platformVariant = 2; // mahog
        }
        else if (tile == TileID.HardenedSand || tile == TileID.Sandstone)
        {
            tileType = TileID.Sandstone;
            checkTypes = [TileID.HardenedSand, TileID.Sandstone];
            platformVariant = 42; // sandstone
        }

        if (checkTypes.Length > 0)
        {
            Point checkPos = origin;
            int checkSegments = 3;
            for (int i = 0; i < checkSegments; i++)
            {
                // check for biome
                Dictionary<ushort, int> tileDictionary = [];
                WorldUtils.Gen(checkPos, new Shapes.Rectangle(sizeX, sizeY / checkSegments), new Actions.TileScanner(checkTypes).Output(tileDictionary));
                int sum = 0;
                for (int s = 0; s < checkTypes.Length; s++)
                    sum += tileDictionary[checkTypes[s]];
                if (sum < sizeRequirement / checkSegments)
                    return false;
                checkPos.Y += (int)(sizeY / checkSegments);
            }
        }
        else
        {
            return false;
        }

        Rectangle rect = new(origin.X, origin.Y, sizeX, sizeY);

        if (WorldUtils.Find(new Point(rect.X - 2, rect.Y - 2), Searches.Chain(new Searches.Rectangle(rect.Width + 4, rect.Height + 4).RequireAll(mode: false), new Conditions.HasLava()), out var _))
            return false;

        if (GenVars.structures != null && !GenVars.structures.CanPlace(rect, BlacklistedTiles, 10))
            return false;
        ShapeData shape = new();

        // dig out shaft
        /*
        Point shaftCenter = origin + new Point(sizeX / 2, topSectionHeight);
        var genshape = new Shapes.Slime((int)(sizeX * 1f), 1f, 0.5f);
        for (int i = 0; i < iter; i++)
        {
            shaftCenter.Y += (int)((1f / iter) * sizeY);
            WorldUtils.Gen(shaftCenter, genshape, Actions.Chain(new Modifiers.Blotches(2, 1, 0.8), new Actions.SetTile(TileID.Stone), new Actions.SetFrames(frameNeighbors: true).Output(shape)));
        }
        */

        // dig out opening
        Point openingCenter = origin + new Point(sizeX / 2, topSectionHeight / 2);
        var genshape = new Shapes.Slime((int)(sizeX * 1.2f), 1f, 0.4f);
        WorldUtils.Gen(openingCenter, genshape, Actions.Chain(new Modifiers.Blotches(3, 3, 0.08), new Actions.ClearTile(frameNeighbors: true).Output(shape)));

        // dig out shaft
        float iter = 36;
        var shaftCenter = origin + new Point(sizeX / 2, topSectionHeight);
            
        for (int i = 0; i < iter; i++)
        {
            float scaler = MathHelper.Lerp(0.6f, 0.32f, (float)i / iter);
            genshape = new Shapes.Slime((int)(sizeX * scaler), 1f, 0.8f);

            shaftCenter.X = origin.X + sizeX / 2 + WorldGen.genRand.Next(-6, 6);
            shaftCenter.Y += (int)(sizeY / iter);
            WorldUtils.Gen(shaftCenter, genshape, Actions.Chain(new Modifiers.Blotches(3, 3, 0.05), new Actions.ClearTile(frameNeighbors: true).Output(shape)));
        }

        // place platform
        Point ropeCenter = origin + new Point(sizeX / 2, topSectionHeight);
        for (int j = -1; j <= 1; j += 2)
        {
            for (int i = 0; i < 100; i++)
            {
                if (j == 1 && i == 0)
                    continue;
                Point p = ropeCenter + new Point(j * i, 0);
                if (Main.tile[p].HasTile)
                    break;
                if (i < 2 || WorldGen.genRand.NextBool(5, 7))
                    WorldGen.PlaceTile(p.X, p.Y, TileID.Platforms, mute: true, style: platformVariant);
            }
        }
        // place rope
        for (int i = -1; i < sizeY + 5; i++)
        {
            Point p = ropeCenter + new Point(0, i);
            if (Main.tile[p].HasTile && Main.tile[p].TileType != TileID.Platforms)
                break;
            if (Main.tile[p].TileType != TileID.Platforms)
                WorldGen.PlaceTile(p.X, p.Y, TileID.Rope, mute: true);
        }

        // place grass
        if (tileType == TileID.Mud)
        {
            for (int i = origin.X - 5; i < origin.X + sizeX + 10; i++)
            {
                for (int j = origin.Y - 5; j < origin.Y + sizeY + 10; j++)
                {
                    WorldGen.SpreadGrass(i, j, TileID.Mud, TileID.JungleGrass);
                }
            }
        }

        GenVars.structures?.AddProtectedStructure(rect, 10);

        return true;
    }
    public static void Mineshafts(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.FargoSeeds.WorldGenMessages.Mineshafts");

        int amt = 15 + WorldGen.GetWorldSize() * 8;

        for (int i = 0; i < amt; i++)
        {
            progress.Set((float)i / amt);

            int attempts = 15000;

            int xBase = Main.maxTilesX * i / amt;
            int xVar = (int)(Main.maxTilesX / amt);


            for (int attempt = 0; attempt < attempts; attempt++)
            {
                int x = xBase + WorldGen.genRand.Next(-xVar, xVar);
                if (x < 200 || x > Main.maxTilesX - 200)
                    continue;
                int y = WorldGen.genRand.Next((int)(GenVars.worldSurfaceHigh + 160), Main.maxTilesY - 100);

                Point origin = new(x, y);
                if (TryPlaceRopeShaft(origin))
                    break;
            }
        }
    }
    public static bool TryPlaceGrandCavern(Point origin, ushort tileType)
    {
        int sizeX;
        int sizeY;
        if (tileType == TileID.Stone || tileType == TileID.IceBlock) // horizontal big
        {
            sizeX = 170;
            sizeY = 75;
        }
        else if (tileType == TileID.Sandstone) // circular
        {
            sizeX = 96;
            sizeY = 96;
        }
        else if (tileType == TileID.Mud) // vertical
        {
            sizeY = 150;
            sizeX = 110;
        }
        else
        {
            return false;
        }

        Point center = origin;
        origin.X -= sizeX / 2;
        origin.Y -= sizeY / 2;

        if (!WorldGen.InWorld(origin.X, origin.Y, 8) || !WorldGen.InWorld(origin.X + sizeX, origin.Y + sizeY, 8))
            return false;

        if (!WorldGen.InWorld(center.X, center.Y, 8))
            return false;

        Rectangle rect = new(origin.X, origin.Y, sizeX, sizeY);

        if (rect.X < 50 || rect.Right > Main.maxTilesX - 50 || rect.Y < 50 || rect.Bottom().Y > Main.maxTilesY - 50)
            return false;

        if (WorldUtils.Find(new Point(rect.X - 2, rect.Y - 2), Searches.Chain(new Searches.Rectangle(rect.Width + 4, rect.Height + 4).RequireAll(mode: false), new Conditions.HasLava()), out var _))
            return false;

        if (GenVars.structures != null && !GenVars.structures.CanPlace(rect, BlacklistedTiles, 10))
            return false;

        ushort tile = Main.tile[center].TileType;
        ushort[] checkTypes = [];

        int sizeTotal = sizeX * sizeY;
        float sizeRequirement = sizeTotal * 0.25f;

        if (tileType == TileID.Stone && (tile == TileID.Stone || tile == TileID.Dirt))
        {
            checkTypes = [TileID.Stone, TileID.Dirt];
        }
        else if (tileType == TileID.IceBlock && (tile == TileID.IceBlock || tile == TileID.SnowBlock))
        {
            checkTypes = [TileID.IceBlock, TileID.SnowBlock];
        }
        else if (tileType == TileID.Mud && (tile == TileID.JungleGrass))
        {
            checkTypes = [TileID.Mud, TileID.JungleGrass];
        }
        else if (tileType == TileID.Sandstone && (tile == TileID.HardenedSand || tile == TileID.Sandstone))
        {
            checkTypes = [TileID.HardenedSand, TileID.Sandstone];
        }

        if (checkTypes.Length > 0)
        {
            Point checkPos = origin;
            int checkSegments = 5;
            for (int i = 0; i < checkSegments; i++)
            {
                // check for biome
                Dictionary<ushort, int> tileDictionary = [];
                WorldUtils.Gen(checkPos, new Shapes.Rectangle((int)((float)sizeX / checkSegments), sizeY), new Actions.TileScanner(checkTypes).Output(tileDictionary));
                int sum = 0;
                for (int s = 0; s < checkTypes.Length; s++)
                    sum += tileDictionary[checkTypes[s]];
                if (sum < sizeRequirement / checkSegments)
                    return false;
                checkPos.X += (int)((float)sizeX / checkSegments);
            }
        }
        else
        {
            return false;
        }

        // carve out cave
        ShapeData shape = new();
        for (int i = 0; i < 210; i++)
        {
            var genshape = new Shapes.Slime((int)(sizeX / 10f), 1f, 1f);
            Point pos = center + WorldGen.genRand.NextVector2Circular(sizeX / 2, sizeY / 2).ToPoint();
            WorldUtils.Gen(pos, genshape, Actions.Chain(new Modifiers.Blotches(1, 1, 0.8), new Actions.ClearTile(frameNeighbors: true).Output(shape)));
        }


        // stalactites
        if (tileType != TileID.Mud && tileType != TileID.Sandstone)
        {
            int stalac = (int)(WorldGen.genRand.NextFloat(0.5f, 1f) * sizeX / 5f);
            for (int i = 0; i < stalac; i++)
            {
                int sWidth = WorldGen.genRand.Next(2, 5);
                float sHeight = WorldGen.genRand.NextFloat(2.5f, 4.5f) * sWidth;
                Point stalPos = new((int)WorldGen.genRand.NextFloat(origin.X + -0.1f * sizeX, origin.X + 1.1f * sizeX), origin.Y + sizeY / 2);
                for (int s = 0; s <= sWidth; s++)
                {
                    stalPos.X += 1;
                    bool valid = true;
                    for (int up = 0; up < sizeY; up++)
                    {
                        if (Main.tile[stalPos].HasTile && Main.tileSolid[Main.tile[stalPos].TileType])
                        {
                            break;
                        }
                        stalPos.Y -= 1;
                        if (up > sizeY * 0.9f)
                            valid = false;
                    }
                    if (!valid)
                        break;
                    float heightScaler = (s - (sWidth / 2f)) / sWidth;
                    int thisHeight = (int)(sHeight * (1f - Math.Abs(heightScaler * 1.3f)));
                    thisHeight += WorldGen.genRand.Next(0, 3);
                    var stalShape = new Shapes.Rectangle(1, thisHeight);
                    WorldUtils.Gen(stalPos, stalShape, Actions.Chain(new Modifiers.Blotches(1, 1, 0.1), new Actions.SetTile(tileType), new Actions.SetFrames(frameNeighbors: true)));
                }
            }
        }

        // add islands
        List<Rectangle> islands = [];
        for (int i = 0; i < 5; i++)
        {
            for (int attempt = 0; attempt < 50; attempt++)
            {
                int isSizer = 15;
                float xScale = WorldGen.genRand.NextFloat(1f, 2f);
                var genshape = new Shapes.Slime(isSizer, xScale, 1f);
                Point pos = center + WorldGen.genRand.NextVector2Circular(sizeX / 2.4f, sizeY / 2.4f).ToPoint();
                Point caveSize = new((int)(isSizer * xScale * 2), isSizer * 2);
                Rectangle island = new(pos.X - caveSize.X / 2, pos.Y - caveSize.Y / 2, caveSize.X, caveSize.Y);
                //island.Inflate(0, 1);
                bool br = false;
                foreach (var otherIsland in islands)
                {
                    if (otherIsland.Intersects(island))
                    {
                        br = true;
                        break;
                    }
                            
                }

                if (br)
                    continue;

                // island
                for (int iz = -3; iz < 4; iz++)
                {
                    var isShape = new Shapes.Slime(isSizer, xScale * 1f * 0.5f, 0.8f * WorldGen.genRand.NextFloat(0.5f, 1f));
                    Point isPos = pos + new Point(iz * (int)((float)isSizer / 5), WorldGen.genRand.Next(0, 6));
                    WorldUtils.Gen(isPos, isShape, Actions.Chain(new Modifiers.Blotches(1, 1, 0.1), new Actions.SetTile(tileType), new Actions.SetFrames(frameNeighbors: true).Output(shape)));
                }
                // point in the middle
                int pointX = WorldGen.genRand.Next(-2, 3);
                for (int p = 0; p < 3; p++)
                {
                    Point pointPos = pos + new Point(pointX, 5 + 3 * p);
                    var pointShape = new Shapes.Slime(7 - 2 * p, xScale * 1.5f, 1f);
                    WorldUtils.Gen(pointPos, pointShape, Actions.Chain(new Modifiers.Blotches(1, 1, 0.1), new Actions.SetTile(tileType), new Actions.SetFrames(frameNeighbors: true).Output(shape)));
                }

                // stalactites
                int stalac = (int)(WorldGen.genRand.NextFloat(0.5f, 1f) * 10f);
                for (int st = 0; st < stalac; st++)
                {
                    int sWidth = WorldGen.genRand.Next(1, 3);
                    float sHeight = WorldGen.genRand.NextFloat(2f, 4f) * sWidth;
                    float stalX = pos.X - caveSize.X / 2f + caveSize.X * 0.1f;
                    stalX += caveSize.X * 0.9f * (float)st / stalac;
                    stalX += WorldGen.genRand.NextFloat(-3f, 3f);
                    Point stalPos = new((int)stalX, pos.Y);
                    for (int s = 0; s <= sWidth; s++)
                    {
                        stalPos.X += 1;
                        bool valid = true;
                        for (int up = 0; up < isSizer; up++)
                        {
                            if (!(Main.tile[stalPos].HasTile && Main.tileSolid[Main.tile[stalPos].TileType]))
                            {
                                stalPos.Y -= 1;
                                break;
                            }
                            stalPos.Y += 1;
                            if (up > isSizer * 0.6f)
                                valid = false;
                        }
                        if (!valid)
                            break;
                        float heightScaler = (s - (sWidth / 2f)) / sWidth;
                        int thisHeight = (int)(sHeight * (1f - Math.Abs(heightScaler * 1.3f)));
                        thisHeight += WorldGen.genRand.Next(0, 3);
                        var stalShape = new Shapes.Rectangle(1, thisHeight);
                        WorldUtils.Gen(stalPos, stalShape, Actions.Chain(new Modifiers.Blotches(1, 1, 0.1), new Actions.SetTile(tileType), new Actions.SetFrames(frameNeighbors: true)));
                    }
                }


                // remove top half
                WorldUtils.Gen(pos, genshape, Actions.Chain(new Modifiers.RectangleMask(-(int)(xScale * isSizer * 2f), (int)(xScale * isSizer * 2f), -(int)(isSizer * 2f), 0), new Actions.ClearTile(frameNeighbors: true)));
                islands.Add(island);

                // random top terrain
                int amt = WorldGen.genRand.Next(2, 5);
                for (int t = 0; t < amt; t++)
                {
                    int terSize = WorldGen.genRand.Next(1, 5);
                    int x = (int)WorldGen.genRand.NextFloat(pos.X - caveSize.X / 2.5f, pos.X + caveSize.X / 2.5f);
                    int y = pos.Y + (int)(terSize / 2f + 1);
                    var terShape = new Shapes.Slime(terSize, 2f, 1f);
                    Point terPos = new(x, y);
                    WorldUtils.Gen(terPos, terShape, Actions.Chain(new Modifiers.Blotches(1, 1, 0.1), new Actions.SetTile(tileType), new Actions.SetFrames(frameNeighbors: true).Output(shape)));
                }
                break;
            }
        }


        // fix walls
        if (tileType == TileID.Sandstone)
        {
            for (int i = 0; i < 210; i++)
            {
                var genshape = new Shapes.Slime((int)(sizeX / 10f), 1f, 1f);
                Point pos = center + WorldGen.genRand.NextVector2Circular(sizeX / 2, sizeY / 2).ToPoint();
                WorldUtils.Gen(pos, genshape, Actions.Chain(new Modifiers.Blotches(1, 1, 0.2), new Actions.PlaceWall(type: WallID.Sandstone).Output(shape)));
            }
        }

        // fix grass
        if (tileType == TileID.Mud) 
        {
            for (int i = origin.X - 5; i < origin.X + sizeX + 10; i++)
            {
                for (int j = origin.Y - 5; j < origin.Y + sizeY + 10; j++)
                {
                    WorldGen.SpreadGrass(i, j, TileID.Mud, TileID.JungleGrass);
                }
            }
        }

        GenVars.structures?.AddProtectedStructure(rect, 20);

        return true;
    }
    public static void GrandCaverns(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.FargoSeeds.WorldGenMessages.GrandCaverns");

        int normalAmt = 4 + WorldGen.GetWorldSize() * 2;

        int xW = (int)((float)Main.maxTilesX / normalAmt);

        bool several = WorldGen.GetWorldSize() >= 1;

        int jglAmt = several ? 2 : 1;

        int tundraAmt = several ? 2 : 1;

        int desertAmt = several ? 2 : 1;

        int totalAmt = normalAmt + jglAmt + tundraAmt + desertAmt;

        for (int i = 0; i < totalAmt; i++)
        {
            progress.Set((float)i / totalAmt);

            int attempts = 50000;
            ushort type = TileID.Stone;

            if (i < normalAmt)
                type = TileID.Stone;
            else if (i < normalAmt + jglAmt)
                type = TileID.Mud;
            else if (i < normalAmt + jglAmt + tundraAmt)
                type = TileID.IceBlock;
            else
                type = TileID.Sandstone;

                

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                int x;
                if (type == TileID.Stone)
                {
                        
                    x = i * xW;
                    x += WorldGen.genRand.Next(-(int)(xW * 0.6f), (int)(xW * 0.6f));
                }
                else
                {
                    x = WorldGen.genRand.Next(500, Main.maxTilesX - 500);
                }
                         
                int y = WorldGen.genRand.Next((int)(GenVars.rockLayerHigh), Main.maxTilesY - 400);

                Point origin = new(x, y);
                if (TryPlaceGrandCavern(origin, type))
                    break;

            }
        }
    }
    public static Point16 GraveyardSize;
    public static void Graveyard(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.FargoSeeds.WorldGenMessages.Graveyard");

        string path = "WorldGeneration/Graveyard";
        Mod mod = FargoSeeds.Mod;
        if (GraveyardSize == Point16.Zero)
            GraveyardSize = StructureHelper.API.Generator.GetStructureDimensions(path, mod);

        int attempts = 15000;

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            int deadzone = 400;
            int x = WorldGen.genRand.NextBool() ? WorldGen.genRand.Next(200, Main.maxTilesX / 2 - deadzone) : WorldGen.genRand.Next(Main.maxTilesX / 2 + deadzone, Main.maxTilesX - 200);
            if (x < 200 || x > Main.maxTilesX - 200)
                continue;
            int checkHeight = 40;
            int y = WorldGen.genRand.Next((int)GenVars.worldSurface - checkHeight, (int)GenVars.worldSurface + checkHeight);

            Point origin = new(x, y);
            if (TryPlaceGraveyard(origin))
                break;
        }
    }
    private static readonly bool[] GraveyardTiles = TileID.Sets.Factory.CreateBoolSet(false, TileID.Grass, TileID.Dirt, TileID.Stone);
    public static bool TryPlaceGraveyard(Point origin)
    {
        int sizeX = GraveyardSize.X;
        int sizeY = GraveyardSize.Y;

        Point left = origin;
        Point right = origin;
        left.X -= sizeX / 2;
        right.X += sizeX / 2;

        // initial check
        if (!WorldGen.InWorld(left.X, left.Y, 8) || !WorldGen.InWorld(right.X, right.Y, 8))
            return false;

        // find ground on both corners; if both match, continue
        left = FindGround(left);
        right = FindGround(right);

        if (!WorldGen.InWorld(left.X, left.Y, 8) || !WorldGen.InWorld(right.X, right.Y, 8))
            return false;

        if (Math.Abs(left.Y - right.Y) > 3)
            return false;

        if (Main.tile[left.X, left.Y + 1].TileType is not TileID.Dirt or TileID.Grass || Main.tile[right.X, right.Y + 1].TileType is not TileID.Dirt or TileID.Grass)
            return false;

        left.Y += 4;
        right.Y += 4;

        // get final rect
        Point16 topLeft = new(left.X, left.Y - sizeY);
        Rectangle rect = new(topLeft.X, topLeft.Y, sizeX, sizeY);

        // other basic checks
        if (GenVars.structures != null && !GenVars.structures.CanPlace(rect, TileID.Sets.GeneralPlacementTiles, 2))
            return false;

        // make sure there's enough graveyard to the left
        int graveyardLength = 30;
        Point pointer = left;
        pointer = FindGround(pointer);
        for (int i = 0; i < graveyardLength; i++)
        {
            int yOld = pointer.Y;
            pointer.X -= 1;
            pointer = FindGround(pointer);
            bool breaker = false;
            if (Math.Abs(yOld - pointer.Y) > 3)
            {
                if (i < graveyardLength * 0.75f)
                    return false;
                breaker = true;
            }
            if (i == graveyardLength - 1 || breaker)
            {
                if (Main.tile[pointer.X + 1, yOld + 1].TileType is not TileID.Dirt or TileID.Grass)
                    return false;
            }
            if (breaker)
            {
                break;
            }
        }


        string path = "WorldGeneration/Graveyard";
        Mod mod = FargoSeeds.Mod;
        StructureHelper.API.Generator.GenerateStructure(path, topLeft, mod);
        GenVars.structures?.AddProtectedStructure(rect, 2);

        // fix grass
        /*
        for (int i = topLeft.X - 2; i < topLeft.X + sizeX + 2; i++)
        {
            for (int j = topLeft.Y - 2; j < topLeft.Y + sizeY + 2; j++)
            {
                WorldGen.SpreadGrass(i, j, TileID.Dirt, TileID.Grass);
            }
        }
        */
        pointer = left;
        pointer = FindGround(pointer);
        int graveNext = Main.rand.Next(3, 6);
        int wall = WallID.MetalFence;

        for (int i = 0; i < graveyardLength; i++)
        {
            int yOld = pointer.Y;
            pointer.X -= 1;
            pointer = FindGround(pointer);
            if (Math.Abs(yOld - pointer.Y) > 3)
                break;
            graveNext--;
            if (graveNext <= 0)
            {
                graveNext = Main.rand.Next(3, 6);
                var gravePointer = pointer;
                gravePointer.Y = Math.Max(FindGround(gravePointer).Y, FindGround(new Point(gravePointer.X - 1, gravePointer.Y)).Y);
                for (int a = 0; a < 2; a++)
                {
                    for (int b = -1; b < 2; b++)
                    {
                        WorldGen.KillTile(gravePointer.X - a, gravePointer.Y + b, noItem: true);
                    }
                }
                    
                WorldGen.PlaceTile(gravePointer.X, gravePointer.Y + 1, TileID.Stone, mute: true);
                WorldGen.PlaceTile(gravePointer.X - 1, gravePointer.Y + 1, TileID.Stone, mute: true);

                int x = gravePointer.X - 1;
                int y = gravePointer.Y;
                WorldGen.PlaceTile(x, y, TileID.Tombstones, true, true, style: WorldGen.genRand.Next(5));

                if (Main.tile[x, y - 1].TileType == TileID.Tombstones)
                {
                    Sign.TextSign(Sign.ReadSign(x, y - 1), "");
                }
            }
            WorldGen.PlaceWall(pointer.X, pointer.Y, wall, true);
            WorldGen.PlaceWall(pointer.X, pointer.Y + 1, wall, true);
            if (WorldGen.genRand.NextBool(2))
            {
                WorldGen.PlaceWall(pointer.X, pointer.Y - 1, wall, true);
                if (WorldGen.genRand.NextBool(2))
                {
                    WorldGen.PlaceWall(pointer.X, pointer.Y - 2, wall, true);
                }
            }
                    
        }

        return true;
    }
}