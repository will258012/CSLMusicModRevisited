using AlgernonCommons;
using ColossalFramework.IO;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;

namespace CSLMusicMod.Patches
{
    [HarmonyPatch]
    public class RadioContentInfoPatch
    {
        /// <summary>
        /// The game usually loads its music from its data directories. This is not compatible with
        /// custom music. This patch loads vanilla music the vanilla way and 
        /// custom music from absolute paths.
        /// </summary>
        [HarmonyPatch(typeof(RadioContentInfo), nameof(RadioContentInfo.ObtainClip))]
        public static bool Prefix(RadioContentInfo __instance, ref WWW __result)
        {
            if (File.Exists(__instance.m_fileName))
            {
                var uri = new Uri(__instance.m_fileName);
                var uristring = uri.AbsoluteUri;
                uristring = uristring.Replace("%20", " ");
                //var uristring = "file://" + this.m_fileName.Replace("\\","/").Replace("#", "%23");

                Logging.Message("Loading custom clip from " + __instance.m_fileName + " (" + uristring + ")");

                __result = new WWW(uristring);
            }
            else
            {
                string text = Path.Combine(DataLocation.gameContentPath, "Radio");
                text = Path.Combine(text, __instance.m_contentType.ToString());
                text = Path.Combine(text, __instance.m_folderName);
                text = Path.Combine(text, __instance.m_fileName);

                Logging.Message("Loading Clip from " + text);
                __result = new WWW("file:///" + text);
            }
            return false;
        }
    }
}

