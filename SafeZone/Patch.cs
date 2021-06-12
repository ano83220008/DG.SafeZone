using DuckGame;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SafeZone
{
    [HarmonyPatch(typeof(GameLevel), nameof(GameLevel.Start))]
    internal static class GameLevel_Start
    {
        private static void Postfix(GameLevel __instance)
        {
            SafeZoneEvent.OnLevelStart(__instance);
        }
    }

    [HarmonyPatch(typeof(GameLevel), nameof(GameLevel.PostDrawLayer))]
    internal static class GameLevel_PostDrawLayer
    {
        static FieldInfo _gameMode = typeof(GameLevel).GetField("_mode", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo _waitDings = typeof(GameMode).GetField("_waitAfterSpawnDings", BindingFlags.NonPublic | BindingFlags.Instance);

        private static void Postfix(GameLevel __instance, Layer layer)
        {
            
            GameMode gm = (GameMode)_gameMode.GetValue(__instance);
            
            int waitAfterDings = (int)_waitDings.GetValue(gm);

            bool waiting = !(waitAfterDings > 2);

            SafeZoneEvent.OnLevelPostDrawLayer(__instance, layer, waiting);
        }
    }
}