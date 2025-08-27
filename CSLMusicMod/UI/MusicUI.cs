using System;
using System.Linq;
using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.Reflection;
using CSLMusicMod.Helpers;

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
        private UIMusicListPanel m_ListPanel;

        private bool m_Initialized = false;
     
        /// <summary>
        /// The current radio panel (from vanilla UI)
        /// Used as cache to prevent expensive FindObjectOfTypeAll calls 
        /// </summary>
        private RadioPanel m_CurrentRadioPanel = null;

        /// <summary>
        /// Gets the current radio panel.
        /// This function is expensive. Only call if necessary!
        /// </summary>
        /// <value>The current radio panel.</value>
        public RadioPanel CurrentRadioPanel
        {
            get
            {
                if (m_CurrentRadioPanel != null)
                    return m_CurrentRadioPanel;
                else
                {
                    var radiopanel = Resources.FindObjectsOfTypeAll<RadioPanel>().FirstOrDefault();
                    m_CurrentRadioPanel = radiopanel;

                    return radiopanel;
                }
            }
        }


        public MusicUI()
        {
        }

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }
      
        private void Initialize()
        {
            //Create ui
            UIView v = UIView.GetAView();
            m_ListPanel = (UIMusicListPanel)v.AddUIComponent(typeof(UIMusicListPanel));
            m_ListPanel.Hide();

            m_Initialized = true;

            CSLMusicMod.Log("Initialized music UI");
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
                if (m_ListPanel != null)
                {
                    m_ListPanel.isVisible = ModOptions.Instance.EnableCustomUI && ModOptions.Instance.MusicListVisible && ReflectionHelper.GetPrivateField<bool>(CurrentRadioPanel, "m_isVisible");
                }
            }
            else
            {
                try
                {
                    Initialize();
                }
                catch(Exception e)
                {
                    Debug.LogError("[CSLMusic] Error while initializing music UI: " + e);
                }
            }

        }

        public void OnDestroy()
        {
            if(m_ListPanel != null)
                Destroy(m_ListPanel);
        }
    }
}

