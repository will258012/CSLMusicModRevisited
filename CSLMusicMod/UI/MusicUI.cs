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

        /// <summary>
        /// Gets or sets the panel's last saved position.
        /// </summary>
        internal static Vector3 SavedPanelPosition
        {
            get => new Vector3(SavedPanelPositionX, SavedPanelPositionY);
            set { SavedPanelPositionX = value.x; SavedPanelPositionY = value.y; }
        }

        /// <summary>
        /// Gets or sets the panel's last saved X position.
        /// </summary>
        public static float SavedPanelPositionX { get; set; } = DefaultPosition.x;

        /// <summary>
        /// Gets or sets the panel's last saved Y position.
        /// </summary>
        public static float SavedPanelPositionY { get; set; } = DefaultPosition.y;
        public static Vector3 DefaultPosition => Vector3.left;

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
            try
            {
                ListPanel = (UIMusicListPanel)UIView.GetAView().AddUIComponent(typeof(UIMusicListPanel));
            }
            catch (System.Exception e)
            {
                Logging.LogException(e, "Failed to initialize ListPanel");
            }
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

