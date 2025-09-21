using AlgernonCommons;
using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;

namespace CSLMusicMod.Helpers
{
    /// <summary>
    /// Contains some helper functions that work the AudioManager class of the game
    /// </summary>
    public static class AudioManagerHelper
    {
        private static readonly Traverse m_audioManager = Traverse.Create(AudioManager.instance);
        internal static readonly Traverse<ushort> m_activeRadioChannel = m_audioManager.Field<ushort>("m_activeRadioChannel");
        private static readonly Traverse<bool> m_musicFileIsRadio = m_audioManager.Field<bool>("m_musicFileIsRadio");
        private static readonly Traverse<AudioManager.AudioPlayer> m_currentRadioPlayer = m_audioManager.Field<AudioManager.AudioPlayer>("m_currentRadioPlayer");
        internal static readonly Traverse<string[]> m_musicFiles = m_audioManager.Field<string[]>("m_musicFiles");

        /// <summary>
        /// Returns the currently active channel data
        /// </summary>
        /// <returns>The active channel.</returns>
        public static RadioChannelData? GetActiveChannelData()
        {
            AudioManager mgr = Singleton<AudioManager>.instance;
            ushort activeChannel = m_activeRadioChannel.Value;

            if (activeChannel >= 0)
            {
                RadioChannelData data = mgr.m_radioChannels[activeChannel];
                return data;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the currently playing song
        /// </summary>
        /// <returns>The active content data.</returns>
        public static RadioContentData? GetActiveContentInfo(RadioChannelData? currentChannel = null)
        {
            if (!currentChannel.HasValue)
                currentChannel = GetActiveChannelData();

            if (currentChannel.HasValue)
            {
                AudioManager mgr = Singleton<AudioManager>.instance;
                return currentChannel.Value.m_currentContent != 0 ? mgr.m_radioContents[currentChannel.Value.m_currentContent] : (RadioContentData?)null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the custom channel info from a vanilla channel info if available.
        /// Otherwise returns null.
        /// </summary>
        /// <returns>The user channel info.</returns>
        /// <param name="info">Info.</param>
        public static UserRadioChannel GetUserChannelInfo(RadioChannelInfo info) => Loading.UserRadioContainer.m_UserRadioDict.TryGetValue(info, out var userchannel) ? userchannel : null;

        /// <summary>
        /// Returns the custom content info from a vanilla content info if available.
        /// Otherwise returns null.
        /// </summary>
        /// <returns>The user content info.</returns>
        /// <param name="info">Info.</param>
        public static UserRadioContent GetUserContentInfo(RadioContentInfo info) => Loading.UserRadioContainer.m_UserContentDict.TryGetValue(info, out var usercontent) ? usercontent : null;

        /// <summary>
        /// Switches to the next station
        /// </summary>
        /// <returns><c>true</c>, if it was possible to switch to the next station, <c>false</c> otherwise.</returns>
        public static bool NextStation()
        {
            if (RadioPanelHelper.CurrentRadioPanel != null)
            {
                RadioChannelInfo current = RadioPanelHelper.m_selectedStation.Value;
                RadioChannelInfo[] stations = RadioPanelHelper.m_stations.Value;

                if (stations != null && stations.Length != 0)
                {
                    int index = current != null ? Array.IndexOf(stations, current) : 0;
                    index = index == -1 ? 0 : (index + 1) % stations.Length;

                    RadioChannelInfo next = stations[index];

                    if (next != null)
                    {
                        RadioPanelHelper.SelectStation(next);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tells the vanilla radio system to issue a rebuild of the internal song list of the active channel 
        /// </summary>
        /// <returns><c>true</c>, if track with rebuild was nexted, <c>false</c> otherwise.</returns>
        public static bool TriggerRebuildInternalSongList()
        {
            // Note: there is GetActiveChannelData. But radio channels are structs, so we need to access directly. 
            AudioManager mgr = Singleton<AudioManager>.instance;
            ushort activeChannel = m_activeRadioChannel.Value;

            if (activeChannel >= 0)
            {
                RadioChannelData data = mgr.m_radioChannels[activeChannel];
                data.m_nextContent = 0;
                mgr.m_radioChannels[activeChannel] = data; // Did you know that you don't need this in C++?

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Switches to the next track.
        /// </summary>
        /// <returns><c>true</c>, if track was nexted, <c>false</c> otherwise.</returns>
        public static bool NextTrack()
        {
            return ModOptions.Instance.EnableSmoothTransitions ? NextTrack_Smooth() : NextTrack_Hard();
        }

        /// <summary>
        /// Switches to the next track. This abruptly stops the current song.
        /// </summary>
        /// <returns><c>true</c>, if it was possible to switch to the next track, <c>false</c> otherwise.</returns>
        public static bool NextTrack_Hard()
        {
            AudioManager mgr = Singleton<AudioManager>.instance;

            if (m_musicFileIsRadio.Value)
            {
                Logging.Message("Radio switches to next track");

                var player = m_currentRadioPlayer.Value;
                player?.m_source.Stop();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Switches to the next track, but smooth 
        /// This does NOT work as intended without RadioContentWatcher's ApplySmoothTransition!
        /// </summary>
        /// <returns><c>true</c>, if track smooth was nexted, <c>false</c> otherwise.</returns>
        public static bool NextTrack_Smooth()
        {
            AudioManager mgr = Singleton<AudioManager>.instance;

            // musicFileIsRadio is false if no radio channel is active. We cannot
            // do anything in this case.
            if (m_musicFileIsRadio.Value)
            {
                ushort activechannel = m_activeRadioChannel.Value;

                if (activechannel >= 0)
                {
                    RadioChannelData data = mgr.m_radioChannels[activechannel];
                    data.m_currentContent = 0;
                    mgr.m_radioChannels[activechannel] = data;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Switches to a specific radio content (music)
        /// </summary>
        /// <returns><c>true</c>, if the switch was successful, <c>false</c> otherwise.</returns>
        /// <param name="info">Info.</param>
        public static bool SwitchToContent(RadioContentInfo info)
        {
            AudioManager mgr = Singleton<AudioManager>.instance;

            // musicFileIsRadio is false if no radio channel is active. We cannot
            // do anything in this case.
            if (m_musicFileIsRadio.Value)
            {
                ushort contentindex = 0;
                bool found = false;

                for (int i = 0; i < mgr.m_radioContentCount; ++i)
                {
                    RadioContentData data = mgr.m_radioContents[i];

                    //Debug.Log("CC: " + data + " + " + data.Info + " == " + info);

                    if (data.Info == info)
                    {
                        contentindex = (ushort)i;
                        //Debug.Log("Found content index for " + info);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Logging.Message("Switching to unloaded music " + info);

                    if (!mgr.CreateRadioContent(out contentindex, info))
                    {
                        Logging.Message("... failed to create content " + info);
                        return false;
                    }
                }

                Logging.Message("Radio switches to track " + info);

                //Debug.Log("Content index: " + contentindex);

                // Next content
                ushort activeChannel = m_activeRadioChannel.Value;

                if (activeChannel >= 0)
                {
                    RadioChannelData data = mgr.m_radioChannels[activeChannel];
                    data.m_currentContent = contentindex;
                    //data.m_nextContent = contentindex;
                    mgr.m_radioChannels[activeChannel] = data;
                    //mgr.m_radioChannels[activechannel].ChangeContent(activechannel);

                    return true;
                }

                //var player = ReflectionHelper.GetPrivateField<AudioManager.AudioPlayer>(mgr, "m_currentRadioPlayer");
                //player.m_source.Stop();

                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if a radio content info is disabled.
        /// </summary>
        /// <returns><c>true</c>, if is marked as disabled, <c>false</c> otherwise.</returns>
        /// <param name="info">Info.</param>
        public static bool ContentIsEnabled(RadioContentInfo info)
        {
            if (!ModOptions.Instance.EnableDisabledContent || ModOptions.Instance.DisabledContent.Count == 0 || info == null)
                return true;

            string id = info.m_folderName + "/" + info.m_fileName;
            return !ModOptions.Instance.DisabledContent.Contains(id);
        }

        /// <summary>
        /// Enables or disables a radio content info
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="enabled">If set to <c>true</c> enabled.</param>
        public static void SetContentEnabled(RadioContentInfo info, bool enabled)
        {
            if (info == null)
                return;
            if (ContentIsEnabled(info) == enabled)
                return;

            string id = info.m_folderName + "/" + info.m_fileName;

            if (enabled)
            {
                ModOptions.Instance.DisabledContent.Remove(id);
            }
            else
            {
                ModOptions.Instance.DisabledContent.Add(id);
            }

            ModOptions.Instance.SaveSettings();

        }

        public static string GetContentName(RadioContentInfo info)
        {
            return info == null ? "<Null>" : string.IsNullOrEmpty(info.m_displayName) ? info.name : info.m_displayName;
        }
        /// <summary>
        /// Gets the progress data of the currently playing track.
        /// </summary>
        /// <param name="time">Current playback time in seconds.</param>
        /// <param name="length">Total length of the track in seconds.</param>
        /// <param name="formattedProgress">Formatted progress string (e.g., "01:23 / 04:56").</param>
        /// <returns>True if successfully retrieved progress data; otherwise, false.</returns>
        public static bool GetCurrentTrackProgress(out float time, out float length, out string formattedProgress)
        {
            time = length = default;
            formattedProgress = null;

            if (!m_musicFileIsRadio.Value)
                return false;

            var player = m_currentRadioPlayer.Value;
            var source = player?.m_source;

            if (source == null || source.clip == null)
                return false;

            time = source.time;
            length = source.clip.length;

            FormatTime(time, out string timeFormatted);
            FormatTime(length, out string lengthFormatted);

            formattedProgress = string.Concat(timeFormatted, " / ", lengthFormatted);
            return true;
        }


        private static void FormatTime(float totalSeconds, out string formatted)
        {
            int seconds = (int)totalSeconds;
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            seconds = seconds % 60;

            if (hours > 0)
            {
                formatted = $"{hours:00}:{minutes:00}:{seconds:00}";
            }
            else
            {
                formatted = $"{minutes:00}:{seconds:00}";
            }
        }

        /// <summary>
        /// Sets the playback position of the current track.
        /// </summary>
        /// <param name="time">The target playback time in seconds.</param>
        /// <remarks>
        /// The time will be clamped to the valid range [0, track length - 1] to ensure 
        /// the audio source can properly handle the seek operation.
        /// </remarks>
        public static void SetTrackProgress(float time)
        {
            AudioManager mgr = Singleton<AudioManager>.instance;
            if (m_musicFileIsRadio.Value)
            {
                var player = m_currentRadioPlayer.Value;
                var source = player?.m_source;
                if (source != null && source.clip != null)
                {
                    source.time = Mathf.Clamp(time, 0f, source.clip.length - 1f);
                }
            }
        }
    }
}

