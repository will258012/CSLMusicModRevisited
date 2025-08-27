using AlgernonCommons;
using AlgernonCommons.Keybinding;
using AlgernonCommons.Notifications;
using AlgernonCommons.Translation;
using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.UI;
using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CSLMusicMod
{
    /// <summary>
    /// Class that wraps the options of the mod.
    /// </summary>
    public class ModOptions
    {
        private static ModOptions _Instance = null;
        public static ModOptions Instance
        {
            get
            {
                if (_Instance == null)
                {
                    //_Instance = new GameObject("CSLMusicMod Settings").AddComponent<ModOptions>();
                    _Instance = new ModOptions();
                    _Instance.LoadSettings();

                }

                return _Instance;
            }
        }

        private Options m_Options = new Options();


        public bool CreateMixChannels
        {
            get
            {
                return m_Options.CreateMixChannels;
            }
            set
            {
                m_Options.CreateMixChannels = value;

            }
        }

        public bool CreateChannelsFromLegacyPacks
        {
            get
            {
                return m_Options.CreateChannelsFromLegacyPacks;
            }
            set
            {
                m_Options.CreateChannelsFromLegacyPacks = value;

            }
        }

        public bool EnableMusicPacks
        {
            get
            {
                return m_Options.EnableMusicPacks;
            }
            set
            {
                m_Options.EnableMusicPacks = value;

            }
        }

        public bool AllowContentMusic
        {
            get
            {
                return m_Options.AllowContentMusic;
            }
            set
            {
                m_Options.AllowContentMusic = value;

            }
        }

        public bool AllowContentTalk
        {
            get
            {
                return m_Options.AllowContentTalk;
            }
            set
            {
                m_Options.AllowContentTalk = value;

            }
        }

        public bool AllowContentBlurb
        {
            get
            {
                return m_Options.AllowContentBlurb;
            }
            set
            {
                m_Options.AllowContentBlurb = value;

            }
        }

        public bool AllowContentBroadcast
        {
            get
            {
                return m_Options.AllowContentBroadcast;
            }
            set
            {
                m_Options.AllowContentBroadcast = value;

            }
        }

        public bool AllowContentCommercial
        {
            get
            {
                return m_Options.AllowContentCommercial;
            }
            set
            {
                m_Options.AllowContentCommercial = value;

            }
        }

        public bool MixContentMusic
        {
            get
            {
                return m_Options.MixContentMusic;
            }
            set
            {
                m_Options.MixContentMusic = value;

            }
        }

        public bool MixContentTalk
        {
            get
            {
                return m_Options.MixContentTalk;
            }
            set
            {
                m_Options.MixContentTalk = value;

            }
        }

        public bool MixContentBlurb
        {
            get
            {
                return m_Options.MixContentBlurb;
            }
            set
            {
                m_Options.MixContentBlurb = value;

            }
        }

        public bool MixContentBroadcast
        {
            get
            {
                return m_Options.MixContentBroadcast;
            }
            set
            {
                m_Options.MixContentBroadcast = value;

            }
        }

        public bool MixContentCommercial
        {
            get
            {
                return m_Options.MixContentCommercial;
            }
            set
            {
                m_Options.MixContentCommercial = value;

            }
        }

        public bool EnableCustomUI
        {
            get
            {
                return m_Options.EnableCustomUI;
            }
            set
            {
                m_Options.EnableCustomUI = value;

            }
        }
        public Shortcut ShortcutNextTrack
        {
            get
            {
                return m_Options.ShortcutNextTrack;
            }
            set
            {
                m_Options.ShortcutNextTrack = value;

            }
        }

        public Shortcut ShortcutNextStation
        {
            get
            {
                return m_Options.ShortcutNextStation;
            }
            set
            {
                m_Options.ShortcutNextStation = value;

            }
        }

        public Shortcut ShortcutOpenRadioPanel
        {
            get
            {
                return m_Options.ShortcutOpenRadioPanel;
            }
            set
            {
                m_Options.ShortcutOpenRadioPanel = value;

            }
        }

        public bool EnableDisabledContent
        {
            get
            {
                return m_Options.EnableDisabledContent;
            }
            set
            {
                m_Options.EnableDisabledContent = value;

            }
        }

        public List<String> DisabledContent
        {
            get
            {
                return m_Options.DisabledContent;
            }
            set
            {
                m_Options.DisabledContent = value;

            }
        }

        public bool EnableContextSensitivity
        {
            get
            {
                return m_Options.EnableContextSensitivity;
            }
            set
            {
                m_Options.EnableContextSensitivity = value;

            }
        }

        public bool EnableAddingContentToVanillaStations
        {
            get
            {
                return m_Options.EnableAddingContentToVanillaStations;
            }
            set
            {
                m_Options.EnableAddingContentToVanillaStations = value;

            }
        }

        public bool EnableSmoothTransitions
        {
            get
            {
                return m_Options.EnableSmoothTransitions;
            }
            set
            {
                m_Options.EnableSmoothTransitions = value;

            }
        }

        public List<String> DisabledRadioStations
        {
            get
            {
                return m_Options.DisabledRadioStations;
            }
            set
            {
                m_Options.DisabledRadioStations = value;

            }
        }

        public bool EnableDebugInfo
        {
            get
            {
                return m_Options.EnableDebugInfo;
            }
            set
            {
                m_Options.EnableDebugInfo = value;

            }
        }

        public bool AddVanillaSongsToMusicMix
        {
            get
            {
                return m_Options.AddVanillaSongsToMusicMix;
            }
            set
            {
                m_Options.AddVanillaSongsToMusicMix = value;

            }
        }

        public bool EnableImprovedRadioStationList
        {
            get
            {
                return m_Options.EnableImprovedRadioStationList;
            }
            set
            {
                m_Options.EnableImprovedRadioStationList = value;

            }
        }

        public bool EnableOpenStationDirButton
        {
            get
            {
                return m_Options.EnableOpenStationDirButton;
            }
            set
            {
                m_Options.EnableOpenStationDirButton = value;
            }
        }

        public bool MusicListVisible
        {
            get
            {
                return m_Options.MusicListVisible;
            }
            set
            {
                m_Options.MusicListVisible = value;

            }
        }

        public bool ImprovedDisableContentUI
        {
            get
            {
                return m_Options.ImprovedDisableContentUI;
            }
            set
            {
                m_Options.ImprovedDisableContentUI = value;

            }
        }

        public static String SettingsFilename
        {
            get
            {
                return Path.Combine(DataLocation.applicationBase, "CSLMusicMod.json");
            }
        }

        public ModOptions()
        {
        }

        //public void Awake()
        //{
        //    DontDestroyOnLoad(this);
        //    LoadSettings();
        //}

        public void SaveSettings()
        {
            try
            {
                StringWriter json = new StringWriter();
                JsonWriter f = new JsonWriter(json)
                {
                    PrettyPrint = true
                };

                JsonMapper.ToJson(m_Options, f);
                File.WriteAllText(SettingsFilename, json.ToString());

            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                CSLMusicMod.Log("Settings saved.");
            }
        }

        public void LoadSettings()
        {
            if (File.Exists(SettingsFilename))
            {
                try
                {
                    String data = File.ReadAllText(SettingsFilename);
                    m_Options = JsonMapper.ToObject<Options>(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
                finally
                {
                    CSLMusicMod.Log("Settings loaded.");
                }
            }
            else
            {

            }
        }

        public class Shortcut
        {
            public KeyCode Key { get; set; }
            public bool ModifierControl { get; set; }
            public bool ModifierAlt { get; set; }
            public bool ModifierShift { get; set; }

            public Shortcut()
            {

            }

            public Shortcut(KeyCode key, bool ctrl, bool alt, bool shift)
            {
                Key = key;
                ModifierAlt = alt;
                ModifierControl = ctrl;
                ModifierShift = shift;
            }

            public override string ToString()
            {
                return string.Format("[Shortcut: Key={0}, ModifierControl={1}, ModifierAlt={2}, ModifierShift={3}]", Key, ModifierControl, ModifierAlt, ModifierShift);
            }

            public Keybinding ToKeybinding() => new Keybinding(Key, ModifierControl, ModifierShift, ModifierAlt);
            public InputKey Encode() => SavedInputKey.Encode(Key, ModifierControl, ModifierShift, ModifierAlt);
            public void SetKey(InputKey inputKey)
            {
                Key = (KeyCode)(inputKey & 0xFFFFFFF);
                ModifierControl = (inputKey & 0x40000000) != 0;
                ModifierShift = (inputKey & 0x20000000) != 0;
                ModifierAlt = (inputKey & 0x10000000) != 0;
            }
        }

        public class ShortcutMapping : OptionsKeymapping
        {
            public Shortcut Shortcut { get; set; }

            public static ShortcutMapping AddKeymapping(UIComponent parent, float xPos, float yPos, string text, Shortcut shoutcut)
            {
                // Basic setup.
                ShortcutMapping newKeymapping = parent.gameObject.AddComponent<ShortcutMapping>();
                newKeymapping.Label = text;
                newKeymapping.Binding = shoutcut.ToKeybinding();
                newKeymapping.Panel.relativePosition = new Vector2(xPos, yPos);
                newKeymapping.Shortcut = shoutcut;

                return newKeymapping;
            }

            public override InputKey KeySetting
            {
                get => base.KeySetting;
                set
                {
                    Shortcut.SetKey(value);
                    ButtonLabel = SavedInputKey.ToLocalizedString("KEYNAME", value);
                }
            }
        }
        public class Options
        {
            public string Language
            {
                get => Translations.CurrentLanguage;
                set => Translations.CurrentLanguage = value;
            }
            public string WhatsNewVersion
            {
                get => WhatsNew.LastNotifiedVersionString;
                set => WhatsNew.LastNotifiedVersionString = value;
            }

            public bool CreateMixChannels { get; set; }
            public bool CreateChannelsFromLegacyPacks { get; set; }
            public bool EnableMusicPacks { get; set; }

            public bool AllowContentMusic { get; set; }
            public bool AllowContentBlurb { get; set; }
            public bool AllowContentTalk { get; set; }
            public bool AllowContentCommercial { get; set; }
            public bool AllowContentBroadcast { get; set; }

            public bool EnableCustomUI { get; set; }

            public bool MixContentMusic { get; set; }
            public bool MixContentBlurb { get; set; }
            public bool MixContentTalk { get; set; }
            public bool MixContentCommercial { get; set; }
            public bool MixContentBroadcast { get; set; }
            public Shortcut ShortcutNextTrack { get; set; }
            public Shortcut ShortcutNextStation { get; set; }
            public Shortcut ShortcutOpenRadioPanel { get; set; }

            public List<String> DisabledContent { get; set; }
            public bool EnableDisabledContent { get; set; }

            public bool EnableContextSensitivity { get; set; }
            public double ContentWatcherInterval { get; set; }

            public bool EnableAddingContentToVanillaStations { get; set; }

            public bool EnableSmoothTransitions { get; set; }

            public List<String> DisabledRadioStations { get; set; }
            public bool EnableDebugInfo
            {
                get => Logging.DetailLogging;
                set => Logging.DetailLogging = value;
            }
            public bool AddVanillaSongsToMusicMix { get; set; }
            public bool EnableImprovedRadioStationList { get; set; }
            public bool EnableOpenStationDirButton { get; set; }

            public bool MusicListVisible { get; set; }

            public bool ImprovedDisableContentUI { get; set; }

            public Options()
            {
                CreateMixChannels = true;
                MixContentBlurb = false;
                MixContentBroadcast = false;
                MixContentCommercial = false;
                MixContentMusic = true;
                MixContentTalk = false;

                CreateChannelsFromLegacyPacks = true;
                EnableMusicPacks = true;
                AllowContentMusic = true;
                AllowContentBlurb = true;
                AllowContentTalk = true;
                AllowContentCommercial = true;
                AllowContentBroadcast = true;
                EnableCustomUI = true;

                ShortcutNextTrack = new Shortcut(KeyCode.N, false, false, false);
                ShortcutNextStation = new Shortcut(KeyCode.N, true, false, false);
                ShortcutOpenRadioPanel = new Shortcut(KeyCode.M, false, false, false);

                DisabledContent = new List<string>();
                EnableDisabledContent = true;

                EnableContextSensitivity = true;

                EnableAddingContentToVanillaStations = true;

                EnableSmoothTransitions = true;

                DisabledRadioStations = new List<string>();

                AddVanillaSongsToMusicMix = true;
                EnableImprovedRadioStationList = true;
                EnableOpenStationDirButton = false;

                MusicListVisible = true;

                ImprovedDisableContentUI = true;
            }
        }
    }
}

