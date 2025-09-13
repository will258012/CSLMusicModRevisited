using AlgernonCommons;
using ColossalFramework.UI;
using CSLMusicMod.Helpers;
using UnityEngine;

namespace CSLMusicMod.UI
{
    /// <summary>
    /// Behavior that handles all UI
    /// </summary>
    public class MusicUI : MonoBehaviour
    {
        /// <summary>
        /// The list panel that appears if the radio button is extended
        /// </summary>
        public UIMusicListPanel ListPanel { get; private set; }

        private bool m_Initialized = false;
        private const float m_updateInterval = .1f;
        private float m_nextUpdateTime = default;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            Initialize();
        }
        private void Initialize()
        {
            ListPanel = (UIMusicListPanel)UIView.GetAView().AddUIComponent(typeof(UIMusicListPanel));
            Logging.Message("Initialized music UI");
            m_Initialized = true;
        }
        public void LocaleChanged()
        {
            OnDestroy();
            Initialize();
        }

        public void Update()
        {
            if (!m_Initialized || SimulationManager.instance.ForcedSimulationPaused) return;

            if (Time.time < m_nextUpdateTime) return;

            m_nextUpdateTime = Time.time + m_updateInterval;

            if (ListPanel != null)
            {
                ListPanel.isVisible = ModOptions.Instance.EnableCustomUI && ModOptions.Instance.MusicListVisible && RadioPanelHelper.m_isVisible.Value;
                if (ListPanel.isVisible)
                {
                    ListPanel.UpdateVolumeSliderTooltip();
                    ListPanel.UpdateProgressSlider();
                }
            }
        }
        public void OnDestroy()
        {
            if (ListPanel != null)
                Destroy(ListPanel);
        }
    }
}

