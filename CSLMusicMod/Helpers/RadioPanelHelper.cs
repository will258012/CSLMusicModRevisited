using AlgernonCommons;
using ColossalFramework.UI;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace CSLMusicMod.Helpers
{
    [ReflectionHelper.UsedReflection]
    public static class RadioPanelHelper
    {
        static RadioPanelHelper() => _ = CurrentRadioPanel;
        /// <summary>
        /// Gets the current radio panel.
        /// This function is expensive. Only call if necessary!
        /// </summary>
        /// <value>The current radio panel.</value>
        public static RadioPanel CurrentRadioPanel
        {
            get
            {
                if (m_CurrentRadioPanel == null)
                {
                    m_CurrentRadioPanel = Resources.FindObjectsOfTypeAll<RadioPanel>().FirstOrDefault();
                    if (m_CurrentRadioPanel != null)
                    {
                        m_traverse = Traverse.Create(m_CurrentRadioPanel);
                        m_radioPanel = m_traverse.Field<UIPanel>("m_radioPanel");
                        m_radioList = m_traverse.Field<UIPanel>("m_radioList");
                        m_isVisible = m_traverse.Field<bool>("m_isVisible");
                        m_selectedStation = m_traverse.Field<RadioChannelInfo>("m_selectedStation");
                        m_stations = m_traverse.Field<RadioChannelInfo[]>("m_stations");
                    }
                }
                return m_CurrentRadioPanel;
            }
        }

        /// <summary>
        /// The current radio panel (from vanilla UI)
        /// Used as cache to prevent expensive FindObjectOfTypeAll calls 
        /// </summary>
        private static RadioPanel m_CurrentRadioPanel = null;

        internal static RadioChannelInfo[] m_originalStations = null;

        private static Traverse m_traverse = null;
        internal static Traverse<UIPanel> m_radioPanel = null;
        internal static Traverse<UIPanel> m_radioList = null;
        internal static Traverse<bool> m_isVisible = null;
        internal static Traverse<RadioChannelInfo> m_selectedStation = null;
        internal static Traverse<RadioChannelInfo[]> m_stations = null;
        internal static void SelectStation(RadioChannelInfo radioChannelInfo) => m_traverse.Method("SelectStation", radioChannelInfo).GetValue();
        internal static void ApplyStationsDisabling()
        {
            try
            {
                var disabledStations = ModOptions.Instance.DisabledRadioStations;

                var filtered = m_originalStations.Where((station) =>
                {
                    var userChannel = AudioManagerHelper.GetUserChannelInfo(station);
                    if (userChannel != null)
                    {
                        if (!ModOptions.Instance.EnableMusicPacks)
                            return false;
                        if (!ModOptions.Instance.CreateMixChannels && userChannel.m_Name == "CSLMusicMix")
                            return false;
                        if (!ModOptions.Instance.CreateChannelsFromLegacyPacks && userChannel.m_IsLegacyPack)
                            return false;
                    }

                    return !disabledStations.Contains(station.name);
                }).ToArray();

                if (filtered.Length == 0)
                {
                    Logging.Error("Ignoring filter due to all radio stations are disabled");
                    return;
                }

                var currentStation = AudioManagerHelper.GetActiveChannelData()?.Info;
                if (currentStation != null && !filtered.Contains(currentStation))
                    SelectStation(filtered[0]);

                m_radioList.Value.isVisible = false;
                m_stations.Value = filtered;
            }
            catch (System.Exception e)
            {
                Logging.LogException(e, "Could not disable radio stations");
            }
        }
    }
}
