using ColossalFramework;
using CSLMusicMod.Helpers;
using HarmonyLib;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CSLMusicMod.Patches
{
    [HarmonyPatch]
    public class AudioManagerPatches
    {
        /// <summary>
        /// Allows custom playback of broadcasts.
        /// </summary>
        /// <param name="info">The content to be played</param>
        [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.QueueBroadcast))]
        [HarmonyPrefix]
        public static bool QueueBroadcastPatch(AudioManager __instance, RadioContentInfo info)
        {
            if (!ModOptions.Instance.AllowContentBroadcast)
                return false;

            var broadcastQueue = ReflectionHelper.GetPrivateField<FastList<RadioContentInfo>>(__instance, "m_broadcastQueue"); //Why does CO make everything private, so you can't access it ??

            while (!Monitor.TryEnter(broadcastQueue, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                if (broadcastQueue.m_size < 5)
                {
                    for (int i = 0; i < broadcastQueue.m_size; i++)
                    {
                        if (broadcastQueue.m_buffer[i] == info)
                        {
                            return false;
                        }
                    }
                    broadcastQueue.Add(info);
                }
            }
            finally
            {
                Monitor.Exit(broadcastQueue);
            }
            return false;
        }

        /// <summary>
        /// Allows restriction of content to specific songs
        /// </summary>
        /// <returns>The collect radio content info.</returns>
        /// <param name="type">Type.</param>
        /// <param name="channel">Channel.</param>
        [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.CollectRadioContentInfo))]
        [HarmonyPrefix]
        public static bool CollectRadioContentInfoPatch(AudioManager __instance, RadioContentInfo.ContentType type, RadioChannelInfo channel, ref FastList<ushort> __result)
        {
            //Logging.Message("[Internal] Rebuilding the radio content of channel " + channel.GetLocalizedTitle());
            var traverse = new Traverse(__instance);
            // CO makes some things public and other things private. This is completely insane.
            var m_tempRadioContentBuffer = traverse.Field("m_tempRadioContentBuffer").GetValue<FastList<ushort>>();
            var m_radioContentTable = traverse.Field("m_radioContentTable").GetValue<FastList<ushort>[]>();

            m_tempRadioContentBuffer.Clear();

            if (m_radioContentTable == null)
            {
                // Let's all sing the "Expensive Song!" ♬Expensive, Expensive♬ ♩OMG it's so expensive♩ (Rest of lyrics didn't load, yet)
                traverse.Method("RefreshRadioContentTable").GetValue();
                m_radioContentTable = ReflectionHelper.GetPrivateField<FastList<ushort>[]>(__instance, "m_radioContentTable");
            }
            // Get the allowed radio content
            HashSet<RadioContentInfo> disallowed_content = null;
            if (channel != null)
            {
                RadioContentWatcher.DisallowedContentsCache.TryGetValue(channel, out disallowed_content);
            }

            //Debug.Log("[update]" + channel.GetLocalizedTitle() + " | " + allowed_content);
            /*if(allowed_content == null || allowed_content.Count == 0)
            {
                Debug.Log(channel.GetLocalizedTitle() + ": All content enabled!");
            }*/

            int prefabDataIndex = channel.m_prefabDataIndex;
            if (prefabDataIndex != -1)
            {
                int num = (int)(prefabDataIndex * 5 + type);
                if (num < m_radioContentTable.Length)
                {
                    FastList<ushort> fastList = m_radioContentTable[num];
                    if (fastList != null)
                    {
                        for (int i = 0; i < fastList.m_size; i++)
                        {
                            ushort num2 = fastList.m_buffer[i];
                            RadioContentInfo prefab = PrefabCollection<RadioContentInfo>.GetPrefab(num2);

                            if (prefab != null && Singleton<UnlockManager>.instance.Unlocked(prefab.m_UnlockMilestone))
                            {
                                // Filter only content info that should be kept
                                if (disallowed_content == null || disallowed_content.Count == 0 || !disallowed_content.Contains(prefab))
                                {
                                    prefab.m_cooldown = 1000000;
                                    m_tempRadioContentBuffer.Add(num2);
                                }
                            }
                        }
                    }
                }
            }

            for (int j = 0; j < __instance.m_radioContents.m_size; j++)
            {
                RadioContentData.Flags flags = __instance.m_radioContents.m_buffer[j].m_flags;
                if ((flags & RadioContentData.Flags.Created) != RadioContentData.Flags.None)
                {
                    RadioContentInfo info = __instance.m_radioContents.m_buffer[j].Info;
                    if (info != null)
                    {
                        info.m_cooldown = Mathf.Min(info.m_cooldown, __instance.m_radioContents.m_buffer[j].m_cooldown);
                    }
                }
            }

            __result = m_tempRadioContentBuffer;
            return false;
        }
    }
}

