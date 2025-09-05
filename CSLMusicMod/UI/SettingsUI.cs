using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.UI;
using CSLMusicMod.Helpers;
using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static CSLMusicMod.ModOptions;

namespace CSLMusicMod.UI
{
    public class SettingsUI : OptionsPanelBase
    {
        private UITabstrip m_TabStrip;
        private int _tabIndex = 0;
        private const float LeftMargin = 24f;
        private const float Margin = 5f;
        private const float GroupMargin = 40f;
        private const float TitleMargin = 50f;
        private const float SliderMargin = 60f;
        protected override void Setup()
        {
            m_TabStrip = AutoTabstrip.AddTabstrip(this, 0f, 0f, OptionsPanelManager<SettingsUI>.PanelWidth, OptionsPanelManager<SettingsUI>.PanelHeight, out _, 30f);

            AddOptionsInfo(AddTab(Translations.Translate("TAB_INFO")));
            AddOptionsChannels(AddTab(Translations.Translate("TAB_CHANNELS")));
            AddOptionsContent(new UIHelper(AddTab(Translations.Translate("TAB_CONT"), true)));
            AddOptionsShortcuts(AddTab(Translations.Translate("TAB_SHORTCUTS")));
            AddOptionsUI(new UIHelper(AddTab(Translations.Translate("TAB_UI"), true)));

            m_TabStrip.selectedIndex = -1;
            m_TabStrip.selectedIndex = 0;
        }
        private UIPanel AddTab(string name, bool autoLayout = false) => UITabstrips.AddTextTab(m_TabStrip, name, _tabIndex++, out var _, autoLayout: autoLayout);

        private void AddOptionsInfo(UIComponent component)
        {
            var currentY = Margin;
            {
                var language_DropDown = UIDropDowns.AddPlainDropDown(component, LeftMargin, currentY, Translations.Translate("LANGUAGE_CHOICE"), Translations.LanguageList, Translations.Index);
                language_DropDown.eventSelectedIndexChanged += (_, index) =>
                {
                    Translations.Index = index;
                    OptionsPanelManager<SettingsUI>.LocaleChanged();
                    Loading.UI?.LocaleChanged();
                };
                currentY += language_DropDown.height + GroupMargin;
            }
            {
                var title = UISpacers.AddTitleSpacer(component, LeftMargin, currentY, OptionsPanelManager<SettingsUI>.PanelWidth, Translations.Translate("PERF"));
                currentY += title.height + GroupMargin;
                var tip = UISpacers.AddTitle(component, LeftMargin, currentY, Translations.Translate("PERF_TIP"));
                tip.textScale = 1;
                currentY += tip.height + GroupMargin;
            }
            {
                var title = UISpacers.AddTitleSpacer(component, LeftMargin, currentY, OptionsPanelManager<SettingsUI>.PanelWidth, Translations.Translate("CHANNELS_CONT"));
                currentY += title.height + GroupMargin;
                var tip = UISpacers.AddTitle(component, LeftMargin, currentY,
                Translations.Translate("CHANNELS_CONT_TIP"));
                tip.textScale = 1;
                currentY += tip.height + GroupMargin;
            }
            {
                ModOptions options = Instance;
                var title = UISpacers.AddTitleSpacer(component, LeftMargin, currentY, OptionsPanelManager<SettingsUI>.PanelWidth, Translations.Translate("TRBL"));
                currentY += title.height + GroupMargin;
                var tip = UISpacers.AddTitle(component, LeftMargin, currentY,
                Translations.Translate("TRBL_TIP"));
                tip.textScale = 1;
                currentY += tip.height + GroupMargin;

                var logging_CheckBox = UICheckBoxes.AddPlainCheckBox(component, LeftMargin, currentY, Translations.Translate("DETAIL_LOGGING"));
                logging_CheckBox.isChecked = options.EnableDebugInfo;
                logging_CheckBox.eventCheckChanged += (_, isChecked) => options.EnableDebugInfo = isChecked;
            }
        }



        private void AddOptionsChannels(UIComponent component)
        {
            ModOptions options = Instance;

            var scrollPanel = component.AddUIComponent<UIScrollablePanel>();
            scrollPanel.relativePosition = new Vector2(0, Margin);
            scrollPanel.autoSize = false;
            scrollPanel.autoLayout = false;
            scrollPanel.width = component.width - 15f;
            scrollPanel.height = component.height - 15f;
            scrollPanel.clipChildren = true;
            scrollPanel.builtinKeyNavigation = true;
            scrollPanel.scrollWheelDirection = UIOrientation.Vertical;
            scrollPanel.eventVisibilityChanged += (_, isShow) => { if (isShow) scrollPanel.Reset(); };
            UIScrollbars.AddScrollbar(component, scrollPanel);

            var currentY = Margin;
            {
                {
                    var stationNamesDict = new Dictionary<string, string>();

                    string path = Path.Combine(Path.Combine(DataLocation.gameContentPath, "Radio"), "Music");

                    foreach (string d in Directory.GetDirectories(path))
                    {
                        if (Directory.GetFiles(d).Length != 0)
                        {
                            string folderName = Path.GetFileNameWithoutExtension(d);

                            if (!stationNamesDict.ContainsKey(folderName) && folderName != "Christmas")//Skip unreal channels
                            {
                                var friendlyName = Locale.Get("RADIO_CHANNEL_TITLE", folderName);
                                stationNamesDict.Add(folderName, friendlyName);
                            }
                        }
                    }

                    var title = UISpacers.AddTitle(scrollPanel, LeftMargin, currentY, Translations.Translate("VANILLA_CHANNELS"));
                    currentY += title.height + GroupMargin;

                    var sortedNames =
                        stationNamesDict.OrderBy(kvp => kvp.Value, StringComparer.Create(DesktopHelper.GetCorrectCultureInfo(), true)).ToList();

                    foreach (var kvp in sortedNames)
                    {
                        string folderName = kvp.Key;
                        string friendlyName = kvp.Value;

                        var checkBox = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, friendlyName);
                        checkBox.isChecked = !options.DisabledRadioStations.Contains(folderName);
                        checkBox.eventCheckChanged += (_, isChecked) =>
                        {
                            if (isChecked)
                            {
                                options.DisabledRadioStations.Remove(folderName);
                            }
                            else
                            {
                                options.DisabledRadioStations.Add(folderName);
                            }
                        };
                        currentY += checkBox.height + Margin;
                    }
                }
                {
                    var title = UISpacers.AddTitleSpacer(scrollPanel, LeftMargin, currentY, OptionsPanelManager<SettingsUI>.PanelWidth, Translations.Translate("MUSIC_PACKS"));
                    currentY += title.height + GroupMargin;

                    var enableMusicPacks = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("ENABLE_MUSIC_PACKS"));
                    enableMusicPacks.isChecked = options.EnableMusicPacks;
                    enableMusicPacks.eventCheckChanged += (_, isChecked) => options.EnableMusicPacks = isChecked;
                    currentY += enableMusicPacks.height + Margin;

                    var createChannelsFromLegacyPacks = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("CREATE_CHANNELS_FROM_LEGACY"));
                    createChannelsFromLegacyPacks.isChecked = options.CreateChannelsFromLegacyPacks;
                    createChannelsFromLegacyPacks.eventCheckChanged += (_, isChecked) => options.CreateChannelsFromLegacyPacks = isChecked;
                    currentY += createChannelsFromLegacyPacks.height + Margin;
                }
                {
                    var title = UISpacers.AddTitleSpacer(scrollPanel, LeftMargin, currentY, OptionsPanelManager<SettingsUI>.PanelWidth, Translations.Translate("CHANNEL_ALL_CONT"));
                    currentY += title.height + GroupMargin;

                    var createMixChannel = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("CREATE_MIX_CHANNEL"));
                    createMixChannel.isChecked = options.CreateMixChannels;
                    createMixChannel.eventCheckChanged += (_, isChecked) => options.CreateMixChannels = isChecked;
                    currentY += createMixChannel.height + Margin;

                    var addVanillaSongsToMusicMix = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("INCLUDE_VANILLA_SONGS"));
                    addVanillaSongsToMusicMix.isChecked = options.AddVanillaSongsToMusicMix;
                    addVanillaSongsToMusicMix.eventCheckChanged += (_, isChecked) => options.AddVanillaSongsToMusicMix = isChecked;
                    currentY += addVanillaSongsToMusicMix.height + Margin;

                    var mixContentMusic = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("INCLUDE_MUSIC"));
                    mixContentMusic.isChecked = options.MixContentMusic;
                    mixContentMusic.eventCheckChanged += (_, isChecked) => options.MixContentMusic = isChecked;
                    currentY += mixContentMusic.height + Margin;

                    var mixContentBlurb = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("INCLUDE_BLURBS"));
                    mixContentBlurb.isChecked = options.MixContentBlurb;
                    mixContentBlurb.eventCheckChanged += (_, isChecked) => options.MixContentBlurb = isChecked;
                    currentY += mixContentBlurb.height + Margin;

                    var mixContentTalk = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("INCLUDE_TALKS"));
                    mixContentTalk.isChecked = options.MixContentTalk;
                    mixContentTalk.eventCheckChanged += (_, isChecked) => options.MixContentTalk = isChecked;
                    currentY += mixContentTalk.height + Margin;

                    var mixContentCommercial = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("INCLUDE_COMMERCIALS"));
                    mixContentCommercial.isChecked = options.MixContentCommercial;
                    mixContentCommercial.eventCheckChanged += (_, isChecked) => options.MixContentCommercial = isChecked;
                    currentY += mixContentCommercial.height + Margin;

                    var mixContentBroadcast = UICheckBoxes.AddPlainCheckBox(scrollPanel, LeftMargin, currentY, Translations.Translate("INCLUDE_BROADCASTS"));
                    mixContentBroadcast.isChecked = options.MixContentBroadcast;
                    mixContentBroadcast.eventCheckChanged += (_, isChecked) => options.MixContentBroadcast = isChecked;
                    currentY += mixContentBroadcast.height + Margin;
                }
            }
        }

        private void AddOptionsContent(UIHelperBase helper)
        {
            ModOptions options = Instance;

            {
                var subgroup = helper.AddGroup(Translations.Translate("ADD_FEAT"));

                subgroup.AddCheckbox(Translations.Translate("DISABLED_CONT"),
                    options.EnableDisabledContent,
                    new OnCheckChanged(isChecked => options.EnableDisabledContent = isChecked));
                subgroup.AddCheckbox(Translations.Translate("CONT_SENS"),
                    options.EnableContextSensitivity,
                    new OnCheckChanged(isChecked => options.EnableContextSensitivity = isChecked));
                subgroup.AddCheckbox(Translations.Translate("SMOOTH_TRANS"),
                                     options.EnableSmoothTransitions,
                   new OnCheckChanged(isChecked => options.EnableSmoothTransitions = isChecked));
                //subgroup.AddSlider("Context-sensitivity update interval (Needs reload)",
                //           1,
                //           60,
                //           1,
                //           options.ContentWatcherInterval,
                //           new OnValueChanged((float v) =>
                //{
                //    options.ContentWatcherInterval = v;
                //}));
                subgroup.AddCheckbox(Translations.Translate("ADD_CONT_TO_VANILLA"),
                    options.EnableAddingContentToVanillaStations,
                    new OnCheckChanged(isChecked => options.EnableAddingContentToVanillaStations = isChecked));
                subgroup.AddButton(Translations.Translate("RESET_DISABLED"), new OnButtonClicked(() =>
                {
                    options.DisabledContent.Clear();
                    options.SaveSettings();
                }));
            }
            {
                var subgroup = helper.AddGroup(Translations.Translate("RADIO_CONT"));
                subgroup.AddCheckbox(Translations.Translate("ENABLE_MUSIC"),
                    options.AllowContentMusic,
                    new OnCheckChanged(isChecked => options.AllowContentMusic = isChecked));
                subgroup.AddCheckbox(Translations.Translate("ENABLE_BLURBS"),
                    options.AllowContentBlurb,
                    new OnCheckChanged(isChecked => options.AllowContentBlurb = isChecked));
                subgroup.AddCheckbox(Translations.Translate("ENABLE_TALKS"),
                    options.AllowContentTalk,
                    new OnCheckChanged(isChecked => options.AllowContentTalk = isChecked));
                subgroup.AddCheckbox(Translations.Translate("ENABLE_COMMERCIALS"),
                    options.AllowContentCommercial,
                    new OnCheckChanged(isChecked => options.AllowContentCommercial = isChecked));
                subgroup.AddCheckbox(Translations.Translate("ENABLE_BROADCASTS"),
                    options.AllowContentBroadcast,
                    new OnCheckChanged(isChecked => options.AllowContentBroadcast = isChecked));
            }
        }


        private void AddOptionsShortcuts(UIComponent helper)
        {
            ModOptions options = Instance;
            var currentY = LeftMargin;
            var shortcutOpenRadioPanel = ShortcutMapping.AddKeymapping(helper, LeftMargin, currentY, Translations.Translate("SHOUTCUT_OPENPLAYLIST"), options.ShortcutOpenRadioPanel);
            currentY += shortcutOpenRadioPanel.Panel.height + Margin;

            var nextTrack = ShortcutMapping.AddKeymapping(helper, LeftMargin, currentY, Translations.Translate("SHOUTCUT_NEXTTRACK"), options.ShortcutNextTrack);
            currentY += nextTrack.Panel.height + Margin;

            var nextStation = ShortcutMapping.AddKeymapping(helper, LeftMargin, currentY, Translations.Translate("SHOUTCUT_NEXTSTATION"), options.ShortcutNextStation);
        }

        private void AddOptionsUI(UIHelperBase helper)
        {
            ModOptions options = Instance;

            helper.AddCheckbox(Translations.Translate("ENABLE_PLAYLIST"),
                options.EnableCustomUI,
                new OnCheckChanged(isChecked => options.EnableCustomUI = isChecked));
            helper.AddCheckbox(Translations.Translate("IMPROVED_RADIO_SELECTION"),
                options.EnableImprovedRadioStationList,
                new OnCheckChanged(isChecked => options.EnableImprovedRadioStationList = isChecked));
            helper.AddCheckbox(Translations.Translate("OPEN_DIR"),
                options.EnableOpenStationDirButton,
                new OnCheckChanged(isChecked => options.EnableOpenStationDirButton = isChecked));
            helper.AddCheckbox(Translations.Translate("DISABLE_CONT_CHECKBOXES"),
                options.ImprovedDisableContentUI,
                new OnCheckChanged(isChecked => options.ImprovedDisableContentUI = isChecked));
        }
    }
}

