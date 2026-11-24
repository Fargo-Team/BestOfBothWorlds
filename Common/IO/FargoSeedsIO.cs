using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace FargoSeeds.Common.IO;

public static class FargoSeedsIO
{
    public static string SavePath => Path.Combine(Main.SavePath, "BestOfBothWorlds");
}
