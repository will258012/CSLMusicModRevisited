using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace CSLMusicMod.Patches
{
    /// <summary>
    /// Used for detours of RadioChannelInfo.
    /// </summary>
    [HarmonyPatch]
    public class RadioChannelInfoPatch
    {

        /// <summary>
        /// The game usually translates all radio channel names with a translation table.
        /// If a name is not in this table, an error occurs. This detour looks if the station is custom
        /// and returns the correct title.
        /// </summary>
        [HarmonyPatch(typeof(RadioChannelInfo), nameof(RadioChannelInfo.GetLocalizedTitle))]
        public static bool Prefix(RadioChannelInfo __instance, ref string __result)
        {
            UserRadioCollection collection = Resources.FindObjectsOfTypeAll<UserRadioCollection>().FirstOrDefault();

            if (collection != null && collection.m_Stations.ContainsKey(__instance.name))
            {
                __result = __instance.name;
            }
            else
            {
                __result = ColossalFramework.Globalization.Locale.Get("RADIO_CHANNEL_TITLE", __instance.gameObject.name);
            }
            return false;
        }
    }
}

