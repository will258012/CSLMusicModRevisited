using AlgernonCommons;
using ColossalFramework;
using ColossalFramework.UI;
using CSLMusicMod.Helpers;
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

        private ushort currentChannelIndex = 0;
        private RadioChannelInfo currentChannelInfo = null;
        private UserRadioChannel currentUserChannel = null;
        private string[] musicFilesBackup = null;
        private HashSet<UserRadioContent> cachedApplyingSongs = null;

        public void Start()
        {
            if (musicFilesBackup == null)
            {
                musicFilesBackup = AudioManagerHelper.m_musicFiles.Value;
            }

            AudioManager.instance.m_radioContentChanged += OnContentChanged;
            InvokeRepeating(nameof(ApplyDisallowedContentRestrictions), 1f, 5f);
            InvokeRepeating(nameof(ReevaluateCache), 1f, 10f);
        }

        public void OnDestroy()
        {
            DisallowedContentsCache.Clear();

            if (musicFilesBackup != null)
            {
                AudioManagerHelper.m_musicFiles.Value = musicFilesBackup;
            }

            AudioManager audioManager = AudioManager.instance;
            if (audioManager != null)
            {
                audioManager.m_radioContentChanged -= OnContentChanged;
            }

            CancelInvoke(nameof(ApplyDisallowedContentRestrictions));
            CancelInvoke(nameof(ReevaluateCache));
        }
        public bool IsContentDisallowed(RadioContentInfo content)
        {
            UpdateCache(content);
            return DisallowedContentsCache.TryGetValue(currentChannelInfo, out var result) && result.Contains(content);
        }
        public void UpdateCache(params RadioContentInfo[] contents)
        {
            if (currentChannelInfo == null || contents == null) return;

            if (!DisallowedContentsCache.TryGetValue(currentChannelInfo, out var disallowed))
            {
                disallowed = new HashSet<RadioContentInfo>();
                DisallowedContentsCache[currentChannelInfo] = disallowed;
            }


            if (currentUserChannel != null)
            {
                var allowedsongs = currentUserChannel.GetApplyingSongs();
                // If the channel is a custom channel, we can check for context and for content disabling
                // The method returns NULL if all songs apply!
                if (allowedsongs != null)
                {
                    foreach (var content in contents)
                    {
                        if (content == null) continue;

                        var isDisallowed = false;

                        var userContent = AudioManagerHelper.GetUserContentInfo(content);
                        bool isInContext = !ModOptions.Instance.EnableContextSensitivity || allowedsongs.Contains(userContent);
                        bool isEnabled = AudioManagerHelper.ContentIsEnabled(content);

                        isDisallowed = !isInContext || !isEnabled;

                        if (isDisallowed)
                            disallowed.Add(content);
                        else
                            disallowed.Remove(content);
                    }
                }
                else
                    foreach (var content in contents)
                    {
                        if (content == null) continue;
                        if (content.m_radioChannels == null || !content.m_radioChannels.Contains(currentChannelInfo)) continue;

                        if (AudioManagerHelper.ContentIsEnabled(content))
                            disallowed.Remove(content);
                        else disallowed.Add(content);
                    }
            }
            else
            {
                // If the channel is a vanilla channel, we can still disable content
                foreach (var content in contents)
                {
                    if (content == null) continue;

                    if (AudioManagerHelper.ContentIsEnabled(content))
                        disallowed.Remove(content);
                    else disallowed.Add(content);
                }
            }

        }
        private void ReevaluateCache()
        {
            if (currentUserChannel != null)
            {
                var applyingSongs = currentUserChannel.GetApplyingSongs();
                if (applyingSongs != cachedApplyingSongs)
                {
                    cachedApplyingSongs = applyingSongs;
                    UpdateCache(currentUserChannel.m_Content
                            .Select(content => content.m_VanillaContentInfo)
                            .Where(content => content != null)
                            .ToArray());
                    return;
                }
            }
            if (!DisallowedContentsCache.TryGetValue(currentChannelInfo, out var disallowed) || disallowed.Count == 0) return;

            UpdateCache(disallowed.ToArray());
        }

        private bool UpdateCurrentChannelCache(RadioChannelData channel)
        {
            if (currentChannelIndex == channel.m_infoIndex &&
                currentChannelInfo == channel.Info)
                return false;

            currentChannelIndex = channel.m_infoIndex;
            currentChannelInfo = channel.Info;
            currentUserChannel = AudioManagerHelper.GetUserChannelInfo(currentChannelInfo);
            cachedApplyingSongs = null;
            return true;
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
            var currentchannel = AudioManagerHelper.GetActiveChannelData();

            if (!currentchannel.HasValue)
                return;

            var currentcontent = AudioManagerHelper.GetActiveContentInfo(currentchannel);

            if (!currentcontent.HasValue)
                return;

            var contentInfo = currentcontent.Value.Info;

            if (!IsContentDisallowed(contentInfo))
                return;

            if (ModOptions.Instance.EnableDebugInfo && DisallowedContentsCache.TryGetValue(currentChannelInfo, out var disallowed))
            {
                var builder = new System.Text.StringBuilder();
                builder.AppendLine($"Disallowed content for {currentChannelInfo.name} :");
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
        public void OnContentChanged()
        {
            var channel = AudioManagerHelper.GetActiveChannelData();

            if (!channel.HasValue)
                return;

            if (!UpdateCurrentChannelCache(channel.Value))
                return;

            if (currentUserChannel == null)
            {
                var contents = Enumerable
                    .Range(0, PrefabCollection<RadioContentInfo>.PrefabCount())
                    .Select(index => PrefabCollection<RadioContentInfo>.GetPrefab((uint)index))
                    .Where(content =>
                        content != null &&
                        content.m_radioChannels != null &&
                        content.m_radioChannels.Contains(currentChannelInfo))
                    .ToArray();

                UpdateCache(contents);
            }
            else
            {
                UpdateCache(currentUserChannel.m_Content
                        .Select(content => content.m_VanillaContentInfo)
                        .Where(content => content != null)
                        .ToArray());
            }

            if (!ModOptions.Instance.EnableSmoothTransitions)
                return;

            if (channel.Value.m_flags.IsFlagSet(RadioChannelData.Flags.PlayDefault) && channel.Value.Info.name == "Default")
            {
                if (musicFilesBackup != null)
                {
                    AudioManagerHelper.m_musicFiles.Value = musicFilesBackup;
                }
            }
            else
            {
                AudioManagerHelper.m_musicFiles.Value = null;
            }
        }
        private IEnumerator NextTrack_Hard()
        {
            yield return new WaitForSeconds(0.05f);
            AudioManagerHelper.NextTrack_Hard();
        }
    }
}

