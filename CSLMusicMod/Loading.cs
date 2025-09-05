using AlgernonCommons;
using AlgernonCommons.Patching;
using ColossalFramework.IO;
using CSLMusicMod.UI;
using HarmonyLib;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// The main class for loading the mod.
    /// </summary>
    public class Loading : PatcherLoadingBase<SettingsUI, PatcherBase>
    {
        public static UserRadioCollection UserRadioContainer;
        public static ChannelInitializer StationContainer;
        public static ContentInitializer ContentContainer;
        public static MusicUI UI;
        public static ShortcutHandler UIShortcutHandler;
        public static RadioContentWatcher DisabledContentContainer;

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            if (!Directory.Exists(UserRadioCollection.GameDirUserCollectionDirectory))
            {
                try
                {
                    Directory.CreateDirectory(UserRadioCollection.GameDirUserCollectionDirectory);
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "Could not create CSLMusicMod_Music directory");
                }
            }

            if (UserRadioContainer == null)
            {
                UserRadioContainer = new GameObject("CSLMusicMod_Definitions").AddComponent<UserRadioCollection>();
            }
            if (StationContainer == null)
            {
                StationContainer = new GameObject("CSLMusicMod_Stations").AddComponent<ChannelInitializer>();
            }
            if (ContentContainer == null)
            {
                ContentContainer = new GameObject("CSLMusicMod_Content").AddComponent<ContentInitializer>();
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            Logging.Message("Got OnLevelLoaded: " + mode);

            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario)
            {
                Logging.Message("Level loaded. Loading mod components.");

                RemoveUnsupportedContent();
                UserRadioContainer.CollectPostLoadingData();
                ExtendVanillaContent();

                // Build UI and other post loadtime
                if (UI == null && ModOptions.Instance.EnableCustomUI)
                {
                    UI = new GameObject("CSLMusicMod_UI").AddComponent<MusicUI>();
                }
                if (UIShortcutHandler == null)
                {
                    UIShortcutHandler = new GameObject("CSLMusicMod_UIShortcutHandler").AddComponent<ShortcutHandler>();
                }
                if (DisabledContentContainer == null)
                {
                    DisabledContentContainer = new GameObject("CSLMusicMod_DisabledContent").AddComponent<RadioContentWatcher>();
                }

                if (Logging.DetailLogging)
                    try
                    {
                        DebugOutput();
                    }
                    catch (Exception ex)
                    {
                        Logging.LogException(ex, "DebugOutput Error");
                    }
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
            if (UI != null)
            {
                UnityEngine.Object.Destroy(UI.gameObject);
                UI = null;
            }
            if (UIShortcutHandler != null)
            {
                UnityEngine.Object.Destroy(UI.gameObject);
                UIShortcutHandler = null;
            }
            if (StationContainer != null)
            {
                UnityEngine.Object.Destroy(StationContainer.gameObject);
                StationContainer = null;
            }
            if (ContentContainer != null)
            {
                UnityEngine.Object.Destroy(ContentContainer.gameObject);
                ContentContainer = null;
            }
            if (UserRadioContainer != null)
            {
                UnityEngine.Object.Destroy(UserRadioContainer.gameObject);
                UserRadioContainer = null;
            }
            if (DisabledContentContainer != null)
            {
                UnityEngine.Object.Destroy(DisabledContentContainer.gameObject);
                UserRadioContainer = null;
            }
        }

        private void RemoveUnsupportedContent()
        {
            // Apply filtering after loading
            for (uint i = 0; i < PrefabCollection<RadioChannelInfo>.PrefabCount(); ++i)
            {
                RadioChannelInfo info = PrefabCollection<RadioChannelInfo>.GetPrefab(i);

                if (info == null)
                    continue;

                RemoveUnsupportedContent(info);
            }
        }

        private void DebugOutput()
        {
            for (uint i = 0; i < PrefabCollection<RadioChannelInfo>.PrefabCount(); ++i)
            {
                var info = PrefabCollection<RadioChannelInfo>.GetPrefab(i);
                if (info == null)
                    continue;

                var messageBuilder = new System.Text.StringBuilder(512);
                messageBuilder.AppendLine("[ChannelInfo] " + info.name);
                messageBuilder.AppendLine("Schedule:");

                if (info.m_stateChain != null)
                {
                    foreach (RadioChannelInfo.State s in info.m_stateChain)
                    {
                        messageBuilder.AppendLine($"\t{s.m_contentType} {s.m_minCount} - {s.m_maxCount}");
                    }
                }

                messageBuilder.AppendLine("Content:");

                for (uint j = 0; j < PrefabCollection<RadioContentInfo>.PrefabCount(); ++j)
                {
                    RadioContentInfo content = PrefabCollection<RadioContentInfo>.GetPrefab(j);

                    if (content == null || content.m_radioChannels == null || !content.m_radioChannels.Contains(info))
                        continue;
                    messageBuilder.AppendLine($"\t{content.name} ({content.m_fileName})");
                }

                Logging.Message(messageBuilder);
            }

            var disasterMessageBuilder = new System.Text.StringBuilder();
            disasterMessageBuilder.AppendLine("[DisasterContext] Disaster names: ");
            for (uint i = 0; i < PrefabCollection<DisasterInfo>.PrefabCount(); ++i)
            {
                var info = PrefabCollection<DisasterInfo>.GetPrefab(i);

                if (info == null)
                    continue;

                disasterMessageBuilder.AppendLine(info.name);
            }
            Logging.Message(disasterMessageBuilder);
        }

        /// <summary>
        /// Removes disabled content from this channel.
        /// </summary>
        /// <param name="info">Info.</param>
        private void RemoveUnsupportedContent(RadioChannelInfo info)
        {
            if (info == null)
                return;
            if (info.m_stateChain == null)
                return;

            Logging.Message("Removing unsupported content from " + info);

            var options = ModOptions.Instance;

            List<RadioChannelInfo.State> states = new List<RadioChannelInfo.State>(info.m_stateChain);
            states.RemoveAll(obj =>
            {
                switch (obj.m_contentType)
                {
                    case RadioContentInfo.ContentType.Blurb:
                        if (!options.AllowContentBlurb)
                        {
                            return true;
                        }
                        break;
                    case RadioContentInfo.ContentType.Broadcast:
                        if (!options.AllowContentBroadcast)
                        {
                            return true;
                        }
                        break;
                    case RadioContentInfo.ContentType.Commercial:
                        if (!options.AllowContentCommercial)
                        {
                            return true;
                        }
                        break;
                    case RadioContentInfo.ContentType.Music:
                        if (!options.AllowContentMusic)
                        {
                            return true;
                        }
                        break;
                    case RadioContentInfo.ContentType.Talk:
                        if (!options.AllowContentTalk)
                        {
                            return true;
                        }
                        break;
                }
                return false;
            });

            info.m_stateChain = states.ToArray();
        }

        /// <summary>
        /// Adds music files that are placed in the vanilla directories to the vanilla channels
        /// </summary>
        private void ExtendVanillaContent()
        {
            if (!ModOptions.Instance.EnableAddingContentToVanillaStations)
                return;
            if (!ModOptions.Instance.AddVanillaSongsToMusicMix)
                return;

            for (uint i = 0; i < PrefabCollection<RadioChannelInfo>.PrefabCount(); ++i)
            {
                RadioChannelInfo info = PrefabCollection<RadioChannelInfo>.GetPrefab(i);

                if (info == null || UserRadioContainer.m_UserRadioDict.ContainsKey(info))
                    continue;

                // Collect existing radio content
                var existing = new HashSet<string>();

                for (uint j = 0; j < PrefabCollection<RadioContentInfo>.PrefabCount(); ++j)
                {
                    RadioContentInfo content = PrefabCollection<RadioContentInfo>.GetPrefab(j);

                    if (content == null || content.m_radioChannels == null || !content.m_radioChannels.Contains(info))
                        continue;

                    string text = Path.Combine(DataLocation.gameContentPath, "Radio");
                    text = Path.Combine(text, content.m_contentType.ToString());
                    text = Path.Combine(text, content.m_folderName);
                    text = Path.Combine(text, content.m_fileName);
                    existing.Add(text);
                }

                var validCollectionNames = new HashSet<string>();
                foreach (RadioContentInfo.ContentType type in Enum.GetValues(typeof(RadioContentInfo.ContentType)))
                {
                    validCollectionNames.Add(type + ": " + info.name);
                }

                // Add custom commercials in common folder only if the channel has commercials
                if (info.m_stateChain.Any(state => state.m_contentType == RadioContentInfo.ContentType.Commercial))
                    validCollectionNames.Add("Commercial: Common");

                // Check our collection for non-existing files
                foreach (UserRadioContent userContent in UserRadioContainer.m_Songs.Values)
                {
                    if (!existing.Contains(userContent.m_FileName) && validCollectionNames.Contains(userContent.m_Collection))
                    {
                        userContent.m_VanillaContentInfo.m_radioChannels = userContent.m_VanillaContentInfo.m_radioChannels.AddToArray(info);
                        Logging.Message("[ExtendedVanillaContent] Added " + userContent.m_FileName + " to vanilla station " + info.name);
                    }
                }
            }
        }
    }
}

