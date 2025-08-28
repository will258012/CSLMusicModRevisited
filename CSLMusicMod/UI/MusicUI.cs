using ColossalFramework.UI;
using CSLMusicMod.Helpers;
using System;
using System.Linq;
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
        }
        private void Initialize()
        {
            ListPanel = (UIMusicListPanel)UIView.GetAView().AddUIComponent(typeof(UIMusicListPanel));
            CSLMusicMod.Log("Initialized music UI");
            m_Initialized = true;
        }
        public void LocaleChanged()
        {
            OnDestroy();
            Initialize();
        }

        public void Update()
        {
            if (m_Initialized)
            {
                if (ListPanel != null)
                {
                    ListPanel.isVisible = ModOptions.Instance.EnableCustomUI && ModOptions.Instance.MusicListVisible && ReflectionHelper.GetPrivateField<bool>(AudioManagerHelper.CurrentRadioPanel, "m_isVisible");
                }
            }
            else
            {
                try
                {
                    Initialize();
                }
                catch (Exception e)
                {
                    Debug.LogError("[CSLMusic] Error while initializing music UI: " + e);
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

