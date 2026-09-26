using FargoSeeds.UI.WorldGenMenu;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargoSeeds
{
    public class HardmodeOres : ModSystem
    {
        private static bool bothOres;

        public override void Load()
        {
            IL_WorldGen.SmashAltar += EnableBothOreSets;
        }

        public override void ClearWorld() => bothOres = false;

        // creation-menu choice must survive reloads and hosting on dedicated server
        // clients do not generate altar ores, so this flag needs no custom network packet
        public override void PreWorldGen() => bothOres = WorldGenOptions.BothOres;

        public override void SaveWorldData(TagCompound tag)
        {
            tag["BothOres"] = bothOres;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            bothOres = tag.GetBool("BothOres");
        }

        private static void EnableBothOreSets(ILContext il)
        {
            // reuse vanilla's drunk world ore cycle without changing Main.drunkWorld
            var cursor = new ILCursor(il);
            int matches = 0;
            while (cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdsfld<Main>(nameof(Main.drunkWorld))))
                matches++;

            if (matches != 3)
            {
                var mod = ModContent.GetInstance<FargoSeeds>();
                mod.Logger.Warn($"SmashAltar IL edit failed: expected three drunk-world ore checks, found {matches}.");
                MonoModHooks.DumpIL(mod, il);
                return;
            }

            cursor.Index = 0;
            while (cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdsfld<Main>(nameof(Main.drunkWorld))))
            {
                cursor.EmitDelegate<System.Func<bool, bool>>(drunkWorld => drunkWorld || bothOres);
            }
        }
    }
}
