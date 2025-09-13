using ColossalFramework.UI;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace CSLMusicMod.Helpers
{
    public static class RadioPanelHelper
    {
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
                }
                return m_CurrentRadioPanel;
            }
        }

        /// <summary>
        /// The current radio panel (from vanilla UI)
        /// Used as cache to prevent expensive FindObjectOfTypeAll calls 
        /// </summary>
        private static RadioPanel m_CurrentRadioPanel = null;

        private static readonly Traverse m_traverse = Traverse.Create(CurrentRadioPanel);
        internal static readonly Traverse<UIPanel> m_radioPanel = m_traverse.Field<UIPanel>("m_radioPanel");
        internal static readonly Traverse<UIPanel> m_radioList = m_traverse.Field<UIPanel>("m_radioList");
        internal static readonly Traverse<bool> m_isVisible = m_traverse.Field<bool>("m_isVisible");

        internal static readonly Traverse selectStationMethod = m_traverse.Method("SelectStation");
    }
}
