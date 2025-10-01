using CSLMusicMod.Helpers;
using HarmonyLib;

namespace CSLMusicMod.Patches
{
    [HarmonyPatch]
    public class RadioChannelInfoPatch
    {
        /// <summary>
        /// The game usually translates all radio channel names with a translation table.
        /// If a name is not in this table, an error occurs. This patch looks if the station is custom
        /// and returns the correct title.
        /// </summary>
        [HarmonyPatch(typeof(RadioChannelInfo), nameof(RadioChannelInfo.GetLocalizedTitle))]
        public static bool Prefix(RadioChannelInfo __instance, ref string __result)
        {
            var userStation = AudioManagerHelper.GetUserChannelInfo(__instance);

            __result = userStation != null
                ? userStation.m_DisplayName
                : ColossalFramework.Globalization.Locale.Get("RADIO_CHANNEL_TITLE", __instance.gameObject.name);
            return false;
        }
    }
}

