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
        public static Dictionary<RadioChannelInfo, HashSet<RadioContentInfo>> DisallowedContents { get; private set; } =
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

        /// <summary>
        /// Rebuilds the allowed content for a channel.
        /// </summary>
        /// <param name="channel">Channel.</param>
        public static void RebuildDisallowedContents(RadioChannelInfo channel)
        {
            if (channel == null)
            {
                return;
            }

            if (!DisallowedContents.TryGetValue(channel, out var disallowed))
            {
                disallowed = new HashSet<RadioContentInfo>();
                DisallowedContents[channel] = disallowed;
            }
            else
            {
                disallowed.Clear();
            }

            UserRadioChannel userchannel = AudioManagerHelper.GetUserChannelInfo(channel);

            if (userchannel != null)
            {
                // If the channel is a custom channel, we can check for context and for content disabling
                // The method returns NULL if all songs apply!
                var allowedsongs = userchannel.GetApplyingSongs();

                if (allowedsongs == null)
                {
                    if (ModOptions.Instance.EnableDisabledContent && ModOptions.Instance.DisabledContent.Count != 0)
                    {
                        foreach (UserRadioContent usercontent in userchannel.m_Content)
                        {
                            if (usercontent.m_VanillaContentInfo != null)
                            {
                                bool isenabled = (!ModOptions.Instance.EnableDisabledContent || AudioManagerHelper.ContentIsEnabled(usercontent.m_VanillaContentInfo));

                                if (!isenabled)
                                {
                                    disallowed.Add(usercontent.m_VanillaContentInfo);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (UserRadioContent usercontent in userchannel.m_Content)
                    {
                        if (usercontent.m_VanillaContentInfo != null)
                        {
                            bool isincontext = (!ModOptions.Instance.EnableContextSensitivity || allowedsongs.Contains(usercontent));
                            bool isenabled = (!ModOptions.Instance.EnableDisabledContent || AudioManagerHelper.ContentIsEnabled(usercontent.m_VanillaContentInfo));

                            if (!isincontext || !isenabled)
                            {
                                disallowed.Add(usercontent.m_VanillaContentInfo);
                            }
                        }
                    }
                }
            }
            else
            {
                // If the channel is a vanilla channel, we can still disable content
                AudioManager mgr = Singleton<AudioManager>.instance;

                if (mgr.m_radioContents.m_size > 0)
                {
                    for (int i = 0; i < mgr.m_radioContents.m_size; ++i)
                    {
                        var content = mgr.m_radioContents[i];
                        if (content.Info != null && content.Info.m_radioChannels != null && content.Info.m_radioChannels.Contains(channel))
                        {
                            if (!AudioManagerHelper.ContentIsEnabled(content.Info))
                            {
                                disallowed.Add(content.Info);
                            }
                        }
                    }
                }
            }
            DisallowedContents[channel] = disallowed;
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

            if (currentchannel.HasValue)
            {
                RebuildDisallowedContents(currentchannel.Value.Info);
                RadioContentData? currentcontent = AudioManagerHelper.GetActiveContentInfo();

                if (currentcontent.HasValue && currentcontent.Value.Info != null)
                {

                    if (DisallowedContents.TryGetValue(currentchannel.Value.Info, out var disallowed))
                    {
                        if (ModOptions.Instance.EnableDebugInfo)
                        {
                            var builder = new System.Text.StringBuilder();
                            builder.AppendLine($"Disallowed content for {currentchannel.Value.Info.name} :");
                            foreach (var v in disallowed)
                            {
                                builder.AppendLine(v.name);
                            }
                            Logging.Message(builder.ToString());
                        }

                        if (disallowed != null && disallowed.Contains(currentcontent.Value.Info))
                        {

                            Logging.Message("Skipping " + currentcontent.Value.Info.m_fileName);
                            AudioManagerHelper.TriggerRebuildInternalSongList();

                            if (!ModOptions.Instance.EnableSmoothTransitions)
                                StartCoroutine(NextTrack_Hard());
                            else AudioManagerHelper.NextTrack_Smooth();
                        }
                    }
                }
            }
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

