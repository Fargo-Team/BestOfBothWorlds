using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;

namespace FargoSeeds;

// TODO: Do not godclass IL Edits.
internal sealed class WorldGen_ModifyPass_Corruption_ILEdit : ModSystem
{
    public override void OnModLoad() => WorldGen.ModifyPass(WorldGen.VanillaGenPasses["Corruption"] as PassLegacy, SpawnSafety);
    public static void SpawnSafety(ILContext context)
    {
        ILCursor cursor = new(context);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchStloc(25)))
        {
            FargoSeeds.Mod.Logger.Warn("Both evils spawn safety ILEdit failure on MatchStloc(25)");
            return;
        }
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStloc(25)))
        {
            FargoSeeds.Mod.Logger.Warn("Both evils spawn safety ILEdit failure on MatchStloc(25)");
            return;
        }

        cursor.EmitDelegate((int safeDistance) =>
        {
            if (WorldConfig.Instance.BothEvilsSpawnSafety)
                safeDistance = 200; // from 100
            return safeDistance;
        });
    }
}