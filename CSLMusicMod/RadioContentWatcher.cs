using AlgernonCommons;
using ColossalFramework;
using CSLMusicMod.Helpers;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// A behavior that periodically checks the currently played radio content.
    /// This is used for disabling, context sensitive etc content.
    /// </summary>
    public class RadioContentWatcher : MonoBehaviour
    {
        public static Dictionary<RadioChannelInfo, HashSet<RadioContentInfo>> DisallowedContentsCache { get; private set; } =
            new Dictionary<RadioChannelInfo, HashSet<RadioContentInfo>>();

        private ushort m_currentChannel = 0;
        private string[] m_musicFilesBackup = null;
        public void Start()
        {
            if (m_musicFilesBackup == null)
            {
                AudioManager mgr = Singleton<AudioManager>.instance;
                m_musicFilesBackup = ReflectionHelper.GetPrivateField<string[]>(mgr, "m_musicFiles");
            }

            AudioManager.instance.m_radioContentChanged += ApplySmoothTransition;
            AudioManager.instance.m_radioContentChanged += ApplyDisallowedContentRestrictions;
        }

        public void OnDestroy()
        {
            if (m_musicFilesBackup != null)
            {
                AudioManager mgr = Singleton<AudioManager>.instance;
                ReflectionHelper.SetPrivateField(mgr, "m_musicFiles", m_musicFilesBackup);
            }
            AudioManager.instance.m_radioContentChanged -= ApplySmoothTransition;
            AudioManager.instance.m_radioContentChanged -= ApplyDisallowedContentRestrictions;
        }

        public bool IsContentDisallowed(RadioChannelInfo channel, RadioContentInfo content)
        {
            if (!DisallowedContentsCache.TryGetValue(channel, out var disallowed))
            {
                disallowed = new HashSet<RadioContentInfo>();
                DisallowedContentsCache[channel] = disallowed;
            }

            var userChannel = AudioManagerHelper.GetUserChannelInfo(channel);
            var isDisallowed = false;

            if (userChannel != null)
            {
                var allowedsongs = userChannel.GetApplyingSongs();
                // If the channel is a custom channel, we can check for context and for content disabling
                // The method returns NULL if all songs apply!
                if (allowedsongs != null)
                {
                    var userContent = AudioManagerHelper.GetUserContentInfo(content);
                    bool isInContext = !ModOptions.Instance.EnableContextSensitivity || allowedsongs.Contains(userContent);
                    bool isEnabled = AudioManagerHelper.ContentIsEnabled(content);

                    isDisallowed = !isInContext || !isEnabled;
                }
                else
                    isDisallowed = !AudioManagerHelper.ContentIsEnabled(content);
            }
            else
            {
                // If the channel is a vanilla channel, we can still disable content
                isDisallowed = !AudioManagerHelper.ContentIsEnabled(content);
            }

            if (isDisallowed)
                disallowed.Add(content);
            else
                disallowed.Remove(content);

            return isDisallowed;
        }

        /// <summary>
        /// Applies the content sensitivity
        /// </summary>
        public void ApplyDisallowedContentRestrictions()
        {
            if (!ModOptions.Instance.EnableContextSensitivity && !ModOptions.Instance.EnableDisabledContent)
                return;

            // Find the current content and check if it is in the list of allowed content
            // Otherwise trigger radio content rebuild and stop playback
            RadioChannelData? currentchannel = AudioManagerHelper.GetActiveChannelData();

            if (!currentchannel.HasValue)
                return;

            RadioContentData? currentcontent = AudioManagerHelper.GetActiveContentInfo();

            if (!currentcontent.HasValue)
                return;

            var channelInfo = currentchannel.Value.Info;
            var contentInfo = currentcontent.Value.Info;

            if (channelInfo == null || contentInfo == null || !IsContentDisallowed(channelInfo, contentInfo))
                return;

            if (ModOptions.Instance.EnableDebugInfo && DisallowedContentsCache.TryGetValue(channelInfo, out var disallowed))
            {
                var builder = new System.Text.StringBuilder();
                builder.AppendLine($"Disallowed content for {channelInfo.name} :");
                foreach (var v in disallowed)
                {
                    builder.AppendLine(v.name);
                }
                Logging.Message(builder.ToString());
            }

            Logging.Message("Skipping " + contentInfo.m_fileName);
            AudioManagerHelper.TriggerRebuildInternalSongList();

            if (!ModOptions.Instance.EnableSmoothTransitions)
                StartCoroutine(NextTrack_Hard());
            else AudioManagerHelper.NextTrack_Smooth();
        }
        public void ApplySmoothTransition()
        {
            if (!ModOptions.Instance.EnableSmoothTransitions)
                return;

            RadioChannelData? channel = AudioManagerHelper.GetActiveChannelData();

            if (!channel.HasValue)
                return;

            ushort index = channel.Value.m_infoIndex;

            if (m_currentChannel == index)
                return;

            m_currentChannel = index;

            AudioManager mgr = Singleton<AudioManager>.instance;
            var traverse = Traverse.Create(mgr).Field("m_musicFiles");

            if (channel.Value.m_flags.IsFlagSet(RadioChannelData.Flags.PlayDefault))
            {
                if (m_musicFilesBackup != null)
                {
                    traverse.SetValue(m_musicFilesBackup);
                }
            }
            else
            {
                traverse.SetValue(null);
            }
        }
        private IEnumerator NextTrack_Hard()
        {
            yield return new WaitForSeconds(0.05f);
            AudioManagerHelper.NextTrack_Hard();
        }
    }
}

