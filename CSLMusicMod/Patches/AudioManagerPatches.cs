using AlgernonCommons;
using CSLMusicMod.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CSLMusicMod.Patches
{
    [HarmonyPatch]
    public class AudioManagerPatches
    {
        [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.QueueBroadcast))]
        [HarmonyPrefix]
        public static bool QueueBroadcastPatch(AudioManager __instance, RadioContentInfo info) => ModOptions.Instance.AllowContentBroadcast;

        [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.CollectRadioContentInfo))]
        [HarmonyTranspiler]
        [ReflectionHelper.UsedReflection]
        public static IEnumerable<CodeInstruction> CollectRadioContentInfoPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var resultField = AccessTools.Field(typeof(AudioManager), "m_tempRadioContentBuffer");
            var filterMethod = AccessTools.Method(typeof(AudioManagerPatches), nameof(FilterDisallowedContent));

            for (int i = codes.Count - 2; i >= 0; --i)
            {
                if (codes[i].opcode == OpCodes.Ldfld && Equals(codes[i].operand, resultField) && codes[i + 1].opcode == OpCodes.Ret)
                {
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_2));
                    codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, filterMethod));
                    return codes;
                }
            }

            throw new InvalidOperationException("Failed to patch AudioManager.CollectRadioContentInfo");
        }

        private static FastList<ushort> FilterDisallowedContent(FastList<ushort> contents, RadioChannelInfo channel)
        {
            if (contents == null || channel == null ||
                !RadioContentWatcher.DisallowedContentsCache.TryGetValue(channel, out var disallowed) || disallowed.Count == 0)
                return contents;

            for (int i = contents.m_size - 1; i >= 0; --i)
            {
                var content = PrefabCollection<RadioContentInfo>.GetPrefab(contents.m_buffer[i]);
                if (content != null && disallowed.Contains(content))
                    contents.RemoveAt(i);
            }

            return contents;
        }

        [HarmonyPatch(typeof(RadioChannelData), "FindNextContentInfo")]
        [HarmonyTranspiler]
        [ReflectionHelper.UsedReflection]
        public static IEnumerable<CodeInstruction> FindNextContentInfoPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var collectMethod = AccessTools.Method(
                typeof(AudioManager),
                nameof(AudioManager.CollectRadioContentInfo),
                new[] { typeof(RadioContentInfo.ContentType), typeof(RadioChannelInfo) });
            var fallbackMethod = AccessTools.Method(typeof(AudioManagerPatches), nameof(CollectAvailableRadioContentInfo));

            int replacements = 0;
            foreach (var code in codes)
            {
                if (code.opcode == OpCodes.Callvirt && Equals(code.operand, collectMethod))
                {
                    code.opcode = OpCodes.Call;
                    code.operand = fallbackMethod;
                    ++replacements;
                }
            }

            if (replacements != 1)
                throw new InvalidOperationException("Failed to patch RadioChannelData.FindNextContentInfo");

            return codes;
        }

        private static FastList<ushort> CollectAvailableRadioContentInfo(
            AudioManager audioManager,
            RadioContentInfo.ContentType requestedType,
            RadioChannelInfo channel)
        {
            var contents = audioManager.CollectRadioContentInfo(requestedType, channel);
            if (contents == null || contents.m_size != 0 || channel == null || channel.m_stateChain == null)
                return contents;

            Logging.Message("Requested ", requestedType, " is not avaliable. Falling back to other ContentType");

            var attemptedTypes = new HashSet<RadioContentInfo.ContentType> { requestedType };
            foreach (var state in channel.m_stateChain)
            {
                if (!attemptedTypes.Add(state.m_contentType))
                    continue;

                contents = audioManager.CollectRadioContentInfo(state.m_contentType, channel);
                if (contents != null && contents.m_size != 0)
                    return contents;
            }

            return contents;
        }

    }
}

