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
    public static class ModOptions
    {
        private static Options _Instance = null;
        public static Options Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Options();
                }

                return _Instance;
            }
            private set => _Instance = value;
        }

        public static string SettingsFilename
        {
            get
            {
                return Path.Combine(DataLocation.applicationBase, "CSLMusicMod.json");
            }
        }
        public static void ResetSettings()
        {
            Instance = new Options();
            SaveSettings();
        }
        public static void SaveSettings()
        {
            try
            {
                StringWriter json = new StringWriter();
                JsonWriter f = new JsonWriter(json)
                {
                    PrettyPrint = true
                };

                JsonMapper.ToJson(Instance, f);
                File.WriteAllText(SettingsFilename, json.ToString());

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
            finally
            {
                Logging.Message("Settings saved");
            }
        }

        public static void LoadSettings()
        {
            if (File.Exists(SettingsFilename))
            {
                try
                {
                    string data = File.ReadAllText(SettingsFilename);
                    Instance = JsonMapper.ToObject<Options>(data);
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                }
                finally
                {
                    Logging.Message("Settings loaded.");
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

            public List<string> DisabledContent { get; set; }
            public bool EnableDisabledContent { get; set; }

            public bool EnableContextSensitivity { get; set; }
            public double ContentWatcherInterval { get; set; }

            public bool EnableAddingContentToVanillaStations { get; set; }

            public bool EnableSmoothTransitions { get; set; }

            public List<string> DisabledRadioStations { get; set; }
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

            public float MusicUIPosX
            {
                get => UI.MusicUI.SavedPanelPositionX;
                set => UI.MusicUI.SavedPanelPositionX = value;
            }
            public float MusicUIPosY
            {
                get => UI.MusicUI.SavedPanelPositionY;
                set => UI.MusicUI.SavedPanelPositionY = value;
            }
            public Options()
            {
                Translations.CurrentLanguage = "default";
                Logging.DetailLogging = false;
                WhatsNew.LastNotifiedVersionString = "0.0";

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

                UI.MusicUI.SavedPanelPositionX = UI.MusicUI.DefaultPosition.x;
                UI.MusicUI.SavedPanelPositionY = UI.MusicUI.DefaultPosition.y;
            }
        }
    }
}

