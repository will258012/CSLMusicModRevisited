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
            if (m_Initialized && ListPanel != null)
            {
                ListPanel.isVisible = ModOptions.Instance.EnableCustomUI && ModOptions.Instance.MusicListVisible && ReflectionHelper.GetPrivateField<bool>(AudioManagerHelper.CurrentRadioPanel, "m_isVisible");
            }
        }
        public void OnDestroy()
        {
            if (ListPanel != null)
                Destroy(ListPanel);
        }
    }
}

